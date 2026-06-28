using Microsoft.EntityFrameworkCore;
using Moq;
using BusinessLayer.Service;
using BusinessLayer.Dto.Order;
using DataAccessLayer;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Order;
using DataAccessLayer.Repository.Book;
using DataAccessLayer.Repository.OrderItem;
using DataAccessLayer.Enums; // PaymentMethodEnum, OrderStateEnum

namespace Tests.UnitTests.Servicies;

[TestFixture]
public class OrderServiceTests
{
    private BookHubDbContext _ctx = null!;
    private OrderRepository _orderRepo = null!;
    private BookRepository _bookRepo = null!;
    private OrderItemRepository _orderItemRepo = null!;
    private Mock<IUnitOfWork> _uow = null!;
    private OrderService _service = null!;

    [SetUp]
    public void Setup()
    {
        var opts = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase($"orders_{Guid.NewGuid()}")
            .Options;

        _ctx = new BookHubDbContext(opts);

        // reálné repozitáře
        _orderRepo     = new OrderRepository(_ctx);
        _bookRepo      = new BookRepository(_ctx);
        _orderItemRepo = new OrderItemRepository(_ctx);

        // mock UoW, který deleguje na reálné repo/ctx
        _uow = new Mock<IUnitOfWork>();
        _uow.Setup(u => u.Orders).Returns(_orderRepo);
        _uow.Setup(u => u.Books).Returns(_bookRepo);
        _uow.Setup(u => u.OrderItems).Returns(_orderItemRepo);

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(ct => _ctx.SaveChangesAsync(ct));

        _uow.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<Func<Task<bool>>?>()))
            .Returns<Func<Task>, Func<Task<bool>>?>(
                async (op, verify) =>
                {
                    await op();
                    if (verify != null)
                    {
                        var ok = await verify();
                        if (!ok) throw new Exception("verification failed");
                    }
                });

        _service = new OrderService(_uow.Object);
    }

    [TearDown]
    public void TearDown() => _ctx.Dispose();

    // ---------- helpers ----------
    private User NewUser(string email) => new User { Email = email, PasswordHash = "x" };

    private Book NewBook(string name, float price = 100f) =>
        new Book { Name = name, Description = $"{name} description", Price = price };

    private Order NewOrder(int userId,
                           PaymentMethodEnum pm = (PaymentMethodEnum)1,
                           OrderStateEnum state = OrderStateEnum.Preparing,
                           bool paid = false,
                           float total = 0f) =>
        new()
        {
            UserId = userId,
            PaymentMethod = pm,
            OrderState = state,
            Paid = paid,
            TotalPrice = total
        };

    // ---------- GetOrder ----------
    [Test]
    public async Task GetOrder_ReturnsDto_WhenExists()
    {
        var u = NewUser("a@a.com");
        _ctx.Users.Add(u);
        var o = NewOrder(u.Id);
        _ctx.Orders.Add(o);
        await _ctx.SaveChangesAsync();

        var res = await _service.GetOrder(o.Id);

        Assert.That(res.IsSuccess, Is.True);
        var dto = res.Match(d => d, _ => null!);
        Assert.That(dto.Id, Is.EqualTo(o.Id));
    }

    [Test]
    public async Task GetOrder_ReturnsError_WhenMissing()
    {
        var res = await _service.GetOrder(999);
        Assert.That(res.IsFaulted, Is.True);
        Assert.That(res.ToString(), Does.Contain("Order 999 not found"));
    }

    // ---------- GetOrders ----------
    [Test]
    public async Task GetOrders_ReturnsList_WhenOptionsNull()
    {
        var u = NewUser("b@b.com");
        _ctx.Users.Add(u);
        _ctx.Orders.AddRange(NewOrder(u.Id), NewOrder(u.Id));
        await _ctx.SaveChangesAsync();

        var res = await _service.GetOrders(options: null);

        Assert.That(res.IsSuccess, Is.True);
        var list = res.Match(d => d, _ => new List<OrderDto>());
        Assert.That(list.Count, Is.EqualTo(2));
    }

    // ---------- AddOrder ----------
    [Test]
    public async Task AddOrder_Persists_AndReturnsDto()
    {
        var u = NewUser("c@c.com");
        _ctx.Users.Add(u);
        await _ctx.SaveChangesAsync();

        var req = new CreateOrderDto
        {
            UserId = u.Id,
            PaymentMethod = (PaymentMethodEnum)1,
            OrderState = OrderStateEnum.Preparing,
            Paid = false
        };

        var res = await _service.AddOrder(req);

        Assert.That(res.IsSuccess, Is.True);
        var dto = res.Match(d => d, _ => null!);
        Assert.That(dto.OrderState, Is.EqualTo(OrderStateEnum.Preparing));

        var stored = await _ctx.Orders.FindAsync(dto.Id);
        Assert.That(stored, Is.Not.Null);
        Assert.That(stored!.TotalPrice, Is.EqualTo(0f));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- UpdateOrder ----------
    [Test]
    public async Task UpdateOrder_ChangesFields_AndSaves()
    {
        var u = NewUser("d@d.com");
        _ctx.Users.Add(u);
        var o = NewOrder(u.Id, state: OrderStateEnum.Preparing, paid: false);
        _ctx.Orders.Add(o);
        await _ctx.SaveChangesAsync();

        var req = new UpdateOrderDto
        {
            PaymentMethod = (PaymentMethodEnum)1,
            OrderState = OrderStateEnum.Sending,
            Paid = true
        };

        var res = await _service.UpdateOrder(o.Id, req);

        Assert.That(res.IsSuccess, Is.True);
        var dto = res.Match(d => d, _ => null!);
        Assert.That(dto.OrderState, Is.EqualTo(OrderStateEnum.Sending));
        Assert.That(dto.Paid, Is.True);

        var reloaded = await _ctx.Orders.FindAsync(o.Id);
        Assert.That(reloaded!.OrderState, Is.EqualTo(OrderStateEnum.Sending));
        Assert.That(reloaded!.Paid, Is.True);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateOrder_ReturnsError_WhenNotFound()
    {
        var req = new UpdateOrderDto
        {
            PaymentMethod = (PaymentMethodEnum)1,
            OrderState = OrderStateEnum.Completed,
            Paid = false
        };

        var res = await _service.UpdateOrder(12345, req);
        Assert.That(res.IsFaulted, Is.True);
        Assert.That(res.ToString(), Does.Contain("Order 12345 not found"));
    }

    // ---------- DeleteOrder (soft delete) ----------
    [Test]
    public async Task DeleteOrder_SoftDeletes_WhenExists()
    {
        var u = NewUser("e@e.com");
        _ctx.Users.Add(u);
        var o = NewOrder(u.Id);
        _ctx.Orders.Add(o);
        await _ctx.SaveChangesAsync();

        await _service.DeleteOrder(o.Id);

        var reloaded = await _ctx.Orders.FindAsync(o.Id);
        Assert.That(reloaded, Is.Not.Null);
        Assert.That(reloaded!.IsRemoved, Is.True);

        _uow.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), null), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteOrder_DoesNothing_WhenMissing()
    {
        await _service.DeleteOrder(999);
        _uow.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), null), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task AddOrderItemToOrder_ReturnsError_WhenOrderMissing()
    {
        var res = await _service.AddOrderItemToOrder(new OrderItemDto { OrderId = 999, BookId = 1, Quantity = 1 });
        Assert.That(res.IsFaulted, Is.True);
        Assert.That(res.ToString(), Does.Contain("Order 999 not found"));
    }

    [Test]
    public async Task AddOrderItemToOrder_ReturnsError_WhenBookMissing()
    {
        var u = NewUser("h@h.com");
        _ctx.Users.Add(u);
        var o = NewOrder(u.Id);
        _ctx.Orders.Add(o);
        await _ctx.SaveChangesAsync();

        var res = await _service.AddOrderItemToOrder(new OrderItemDto { OrderId = o.Id, BookId = 999, Quantity = 1 });
        Assert.That(res.IsFaulted, Is.True);
        Assert.That(res.ToString(), Does.Contain("Book 999 not found"));
    }

    // ---------- RemoveOrderItemFromOrder ----------
    [Test]
    public async Task RemoveOrderItemFromOrder_RemovesItem_AndDecreasesTotal()
    {
        var u = NewUser("i@i.com");
        var b = NewBook("RemoveMe", price: 40f);
        _ctx.AddRange(u, b);
        await _ctx.SaveChangesAsync();

        var o = NewOrder(u.Id, total: 80f);
        _ctx.Orders.Add(o);
        await _ctx.SaveChangesAsync();

        var item = new OrderItem { OrderId = o.Id, BookId = b.Id, Quantity = 2, Price = 40f };
        _ctx.OrderItems.Add(item);
        await _ctx.SaveChangesAsync();

        await _service.RemoveOrderItemFromOrder(new OrderItemDto { OrderId = o.Id, BookId = b.Id, Quantity = 2 });

        var reloaded = await _ctx.Orders.FindAsync(o.Id);
        Assert.That(reloaded!.TotalPrice, Is.EqualTo(0f)); // 80 - 2*40

        var removed = await _ctx.OrderItems.FindAsync(item.Id);
        Assert.That(removed, Is.Not.Null);
        Assert.That(removed!.IsRemoved, Is.True);

        _uow.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), null), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- GetOrderItemsByOrderIdAsync ----------
    [Test]
    public async Task GetOrderItemsByOrderIdAsync_ReturnsItems_WhenOrderExists()
    {
        var u = NewUser("j@j.com");
        var b = NewBook("Items", price: 10f);
        _ctx.AddRange(u, b);
        await _ctx.SaveChangesAsync();

        var o = NewOrder(u.Id);
        _ctx.Orders.Add(o);
        await _ctx.SaveChangesAsync();

        _ctx.OrderItems.Add(new OrderItem { OrderId = o.Id, BookId = b.Id, Quantity = 1, Price = 10f });
        await _ctx.SaveChangesAsync();

        var res = await _service.GetOrderItemsByOrderIdAsync(o.Id);

        Assert.That(res.IsSuccess, Is.True);
        var list = res.Match(d => d, _ => new List<OrderItemDto>());
        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list[0].BookId, Is.EqualTo(b.Id));
    }

    [Test]
    public async Task GetOrderItemsByOrderIdAsync_ReturnsError_WhenOrderMissing()
    {
        var res = await _service.GetOrderItemsByOrderIdAsync(777);
        Assert.That(res.IsFaulted, Is.True);
        Assert.That(res.ToString(), Does.Contain("Order 777 not found"));
    }
}
