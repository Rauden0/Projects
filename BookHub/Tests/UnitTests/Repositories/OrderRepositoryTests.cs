using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Order;
using DataAccessLayer.Enums; // OrderStateEnum, PaymentMethodEnum

namespace Tests.UnitTests.Repositories;

[TestFixture]
public class OrderRepositoryTests
{
    private BookHubDbContext _ctx = null!;
    private OrderRepository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        var opts = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _ctx  = new BookHubDbContext(opts);
        _repo = new OrderRepository(_ctx);
    }

    [TearDown]
    public void TearDown() => _ctx.Dispose();

    // ----------------- Helpers -----------------

    private User NewUser(string email) => new User
    {
        Email = email,
        PasswordHash = "x"
    };

    private Book NewBook(string name) => new Book
    {
        Name = name,
        Description = $"{name} description"
    };

    /// <summary>
    /// Vytvoří platnou objednávku včetně povinných polí dle modelu:
    /// Information, PaymentMethod, OrderState.
    /// </summary>
    private Order NewOrder(int userId, string info = "test info",
                           PaymentMethodEnum? pm = null,
                           OrderStateEnum state = OrderStateEnum.Preparing,
                           bool paid = false, float total = 0f) =>
        new Order
        {
            UserId = userId,
            PaymentMethod = pm ?? (PaymentMethodEnum)1, // neznáme konkrétní enumy -> bezpečný cast
            OrderState = state,
            Paid = paid,
            TotalPrice = total
        };

    // ----------------- Base Repository tests -----------------

    [Test]
    public async Task Add_And_GetByIdAsync_ShouldWork()
    {
        var u = NewUser("a@a.com");
        _ctx.Users.Add(u);
        await _ctx.SaveChangesAsync();

        var o = NewOrder(u.Id);
        _repo.Add(o);
        await _ctx.SaveChangesAsync();

        var found = await _repo.GetByIdAsync(o.Id);
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.UserId, Is.EqualTo(u.Id));
        Assert.That(found.OrderState, Is.EqualTo(OrderStateEnum.Preparing));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAll()
    {
        var u = NewUser("b@b.com");
        _ctx.Users.Add(u);
        await _ctx.SaveChangesAsync();

        _ctx.Orders.AddRange(
            NewOrder(u.Id, "info 1"),
            NewOrder(u.Id, "info 2", state: OrderStateEnum.Preparing)
        );
        await _ctx.SaveChangesAsync();

        var all = await _repo.GetAllAsync();
        Assert.That(all.Count, Is.EqualTo(2));
    }

    [Test]
    public void Update_ShouldModifyEntity()
    {
        var u = NewUser("c@c.com");
        _ctx.Users.Add(u);
        _ctx.SaveChanges();

        var o = NewOrder(u.Id, "original");
        _ctx.Orders.Add(o);
        _ctx.SaveChanges();

        // změna stavů a doplnění položek
        var b = NewBook("Dune");
        _ctx.Books.Add(b);
        _ctx.SaveChanges();

        o.OrderState = OrderStateEnum.Preparing;
        o.OrderItems = new List<OrderItem>
        {
            new OrderItem { BookId = b.Id, Quantity = 2 } // záměrně netestujeme Price typ
        };

        _repo.Update(o);
        _ctx.SaveChanges();

        var updated = _ctx.Orders
            .Include(x => x.OrderItems)
            .First(x => x.Id == o.Id);

        Assert.That(updated.OrderState, Is.EqualTo(OrderStateEnum.Preparing));
        Assert.That(updated.OrderItems, Is.Not.Null);
        Assert.That(updated.OrderItems!.Count, Is.EqualTo(1));
        Assert.That(updated.OrderItems!.First().Quantity, Is.EqualTo(2));
    }

    [Test]
    public void Remove_ShouldSoftDelete()
    {
        var u = NewUser("d@d.com");
        _ctx.Users.Add(u);
        _ctx.SaveChanges();

        var o = NewOrder(u.Id);
        _ctx.Orders.Add(o);
        _ctx.SaveChanges();

        _repo.Remove(o);
        _ctx.SaveChanges();

        var reloaded = _ctx.Orders.Find(o.Id);
        Assert.That(reloaded, Is.Not.Null);
        Assert.That(reloaded!.IsRemoved, Is.True);
    }

    [Test]
    public void Query_ShouldFilterAndReturnQueryable()
    {
        var u1 = NewUser("e1@e.com");
        var u2 = NewUser("e2@e.com");
        _ctx.Users.AddRange(u1, u2);
        _ctx.SaveChanges();

        _ctx.Orders.AddRange(
            NewOrder(u1.Id),
            NewOrder(u2.Id, "other")
        );
        _ctx.SaveChanges();

        var q = _repo.Query().Where(o => o.UserId == u1.Id);
        Assert.That(q.Count(), Is.EqualTo(1));
    }

    // ----------------- OrderRepository specific -----------------

    [Test]
    public async Task GetOrdersByUserIdAsync_ShouldReturnOnlyUserOrders_AndIncludeItems()
    {
        var u1 = NewUser("u1@x.com");
        var u2 = NewUser("u2@x.com");
        var b  = NewBook("Foundation");
        _ctx.AddRange(u1, u2, b);
        await _ctx.SaveChangesAsync();

        var o1 = NewOrder(u1.Id, "with item");
        o1.OrderItems = new List<OrderItem>
        {
            new OrderItem { BookId = b.Id, Quantity = 1 }
        };

        var o2 = NewOrder(u1.Id, "empty");
        var o3 = NewOrder(u2.Id, "other user");

        _ctx.Orders.AddRange(o1, o2, o3);
        await _ctx.SaveChangesAsync();

        var user1Orders = await _repo.GetOrdersByUserIdAsync(u1.Id);

        Assert.That(user1Orders.Count, Is.EqualTo(2));
        Assert.That(user1Orders.All(o => o.UserId == u1.Id), Is.True);

        var withItems = user1Orders.First(o => o.Id == o1.Id);
        Assert.That(withItems.OrderItems, Is.Not.Null);
        Assert.That(withItems.OrderItems!.Count, Is.EqualTo(1));
        Assert.That(withItems.OrderItems!.First().BookId, Is.EqualTo(b.Id));
        Assert.That(withItems.OrderItems!.First().Quantity, Is.EqualTo(1));
    }
}
