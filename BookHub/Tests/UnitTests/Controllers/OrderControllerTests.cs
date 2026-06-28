using BookHub.Controller;
using BusinessLayer.Dto.Order;
using BusinessLayer.Facade;
using BusinessLayer.Service;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Moq;

namespace Tests.UnitTests.Controllers;

[TestFixture]
public class OrderControllerTests
{
    private Mock<IOrderService> _serviceMock = null!;
    private Mock<ICheckoutFacade> _checkoutFacadeMock = null!;
    private OrderController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IOrderService>();
        _checkoutFacadeMock = new Mock<ICheckoutFacade>();
        _controller = new OrderController(_serviceMock.Object, _checkoutFacadeMock.Object);
    }

    #region GetOrder
    [Test]
    public async Task GetOrder_ReturnsOk_WhenFound()
    {
        var dto = new OrderDto { Id = 1, TotalPrice = 100 };
        _serviceMock.Setup(s => s.GetOrder(1))
            .ReturnsAsync(new Result<OrderDto>(dto));

        var result = await _controller.GetOrder(1);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(dto));
        _serviceMock.Verify(s => s.GetOrder(1), Times.Once);
    }

    [Test]
    public async Task GetOrder_ReturnsNotFound_WhenMissing()
    {
        _serviceMock.Setup(s => s.GetOrder(42))
            .ReturnsAsync(new Result<OrderDto>(new Exception("Order 42 not found")));

        var result = await _controller.GetOrder(42);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Order 42 not found"));
        _serviceMock.Verify(s => s.GetOrder(42), Times.Once);
    }
    #endregion

    #region GetOrders
    [Test]
    public async Task GetOrders_ReturnsOk_WithList()
    {
        var list = new List<OrderDto> { new() { Id = 1, TotalPrice = 100 } };

        _serviceMock.Setup(s => s.GetOrders(It.IsAny<ODataQueryOptions<OrderDto>>()))
            .ReturnsAsync(new Result<List<OrderDto>>(list));

        var result = await _controller.GetOrders(null);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(list));
        _serviceMock.Verify(s => s.GetOrders(null), Times.Once);
    }

    [Test]
    public async Task GetOrders_ReturnsNotFound_OnError()
    {
        _serviceMock.Setup(s => s.GetOrders(It.IsAny<ODataQueryOptions<OrderDto>>()))
            .ReturnsAsync(new Result<List<OrderDto>>(new Exception("Error fetching orders")));

        var result = await _controller.GetOrders(null);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Error fetching orders"));
        _serviceMock.Verify(s => s.GetOrders(null), Times.Once);
    }
    #endregion

    #region AddOrder (POST)
    [Test]
    public async Task AddOrder_ReturnsOk_WithResultInside()
    {
        var req = new CreateOrderDto { UserId = 7 };
        var dto = new OrderDto { Id = 99, TotalPrice = 250 };
        
        _serviceMock.Setup(s => s.AddOrder(req))
            .ReturnsAsync(new Result<OrderDto>(dto));

        var result = await _controller.AddOrder(req);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        
        Assert.That(ok!.Value, Is.TypeOf<Result<OrderDto>>());
        var inner = (Result<OrderDto>)ok.Value!;
        Assert.That(inner.IsSuccess, Is.True);
        Assert.That(inner.Match(x => x.Id, _ => -1), Is.EqualTo(99));

        _serviceMock.Verify(s => s.AddOrder(req), Times.Once);
    }

    [Test]
    public void AddOrder_BubblesException_WhenServiceThrows()
    {
        var req = new CreateOrderDto { UserId = 7 };
        _serviceMock.Setup(s => s.AddOrder(req))
            .ThrowsAsync(new Exception("Creation failed"));

        var ex = Assert.ThrowsAsync<Exception>(async () => await _controller.AddOrder(req));
        Assert.That(ex!.Message, Is.EqualTo("Creation failed"));
        _serviceMock.Verify(s => s.AddOrder(req), Times.Once);
    }
    #endregion

    #region UpdateOrder (PUT)
    [Test]
    public async Task UpdateOrder_ReturnsOk_WhenUpdated()
    {
        var req = new UpdateOrderDto { OrderState = OrderStateEnum.Completed };
        var dto = new OrderDto { Id = 1, OrderState = OrderStateEnum.Completed, TotalPrice = 100 };

        _serviceMock.Setup(s => s.UpdateOrder(1, req))
            .ReturnsAsync(new Result<OrderDto>(dto));

        var result = await _controller.UpdateOrder(1, req);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(((OrderDto)ok!.Value).OrderState, Is.EqualTo(OrderStateEnum.Completed));
        _serviceMock.Verify(s => s.UpdateOrder(1, req), Times.Once);
    }

    [Test]
    public async Task UpdateOrder_ReturnsNotFound_OnError()
    {
        var req = new UpdateOrderDto { OrderState = OrderStateEnum.Completed };

        _serviceMock.Setup(s => s.UpdateOrder(77, req))
            .ReturnsAsync(new Result<OrderDto>(new Exception("Order 77 not found")));

        var result = await _controller.UpdateOrder(77, req);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Order 77 not found"));
        _serviceMock.Verify(s => s.UpdateOrder(77, req), Times.Once);
    }
    #endregion

    #region DeleteOrder
    [Test]
    public async Task DeleteOrder_ReturnsNoContent_WhenSuccess()
    {
        _serviceMock.Setup(s => s.DeleteOrder(3)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteOrder(3);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        _serviceMock.Verify(s => s.DeleteOrder(3), Times.Once);
    }

    [Test]
    public void DeleteOrder_BubblesException()
    {
        _serviceMock.Setup(s => s.DeleteOrder(3))
            .ThrowsAsync(new Exception("Delete failed"));

        var ex = Assert.ThrowsAsync<Exception>(async () => await _controller.DeleteOrder(3));
        Assert.That(ex!.Message, Is.EqualTo("Delete failed"));
        _serviceMock.Verify(s => s.DeleteOrder(3), Times.Once);
    }
    #endregion

    #region AddOrderItemToOrder (POST /orders/items)
    [Test]
    public async Task AddOrderItemToOrder_ReturnsOk_WhenSuccess()
    {
        var item = new OrderItemDto { OrderId = 1, BookId = 10, Quantity = 2 };
        var dto = new OrderDto { Id = 1, TotalPrice = 120 };

        _serviceMock.Setup(s => s.AddOrderItemToOrder(item))
            .ReturnsAsync(new Result<OrderDto>(dto));

        var result = await _controller.AddOrderItemToOrder(item);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(((OrderDto)ok!.Value).Id, Is.EqualTo(1));
        _serviceMock.Verify(s => s.AddOrderItemToOrder(item), Times.Once);
    }

    [Test]
    public async Task AddOrderItemToOrder_ReturnsNotFound_OnError()
    {
        var item = new OrderItemDto { OrderId = 99, BookId = 10, Quantity = 1 };

        _serviceMock.Setup(s => s.AddOrderItemToOrder(item))
            .ReturnsAsync(new Result<OrderDto>(new Exception("Order 99 not found")));

        var result = await _controller.AddOrderItemToOrder(item);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Order 99 not found"));
        _serviceMock.Verify(s => s.AddOrderItemToOrder(item), Times.Once);
    }
    #endregion

    #region RemoveOrderItemFromOrder (DELETE /orders/items)
    [Test]
    public async Task RemoveOrderItemFromOrder_ReturnsNoContent_WhenSuccess()
    {
        var item = new OrderItemDto { OrderId = 1, BookId = 10, Quantity = 1 };

        _serviceMock.Setup(s => s.RemoveOrderItemFromOrder(item)).Returns(Task.CompletedTask);

        var result = await _controller.RemoveOrderItemFromOrder(item);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        _serviceMock.Verify(s => s.RemoveOrderItemFromOrder(item), Times.Once);
    }

    [Test]
    public void RemoveOrderItemFromOrder_BubblesException()
    {
        var item = new OrderItemDto { OrderId = 1, BookId = 10, Quantity = 1 };

        _serviceMock.Setup(s => s.RemoveOrderItemFromOrder(item))
            .ThrowsAsync(new Exception("Removal failed"));

        var ex = Assert.ThrowsAsync<Exception>(async () => await _controller.RemoveOrderItemFromOrder(item));
        Assert.That(ex!.Message, Is.EqualTo("Removal failed"));
        _serviceMock.Verify(s => s.RemoveOrderItemFromOrder(item), Times.Once);
    }
    #endregion

    #region GetOrderItemInOrderByOrderId (POST /orders/{id}/items)
    [Test]
    public async Task GetOrderItemInOrderByOrderId_ReturnsOk_WithItems()
    {
        var list = new List<OrderItemDto>
        {
            new() { OrderId = 1, BookId = 10, Quantity = 1 }
        };

        _serviceMock.Setup(s => s.GetOrderItemsByOrderIdAsync(1))
            .ReturnsAsync(new Result<List<OrderItemDto>>(list));

        var result = await _controller.GetOrderItemInOrderByOrderId(1);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);

        // controller vrací přesně to, co vrátí service → tedy List<OrderItem> (model)
        Assert.That(ok!.Value, Is.EqualTo(list));
        _serviceMock.Verify(s => s.GetOrderItemsByOrderIdAsync(1), Times.Once);
    }

    [Test]
    public async Task GetOrderItemInOrderByOrderId_ReturnsNotFound_OnError()
    {
        _serviceMock.Setup(s => s.GetOrderItemsByOrderIdAsync(77))
            .ReturnsAsync(new Result<List<OrderItemDto>>(new Exception("Order 77 not found")));

        var result = await _controller.GetOrderItemInOrderByOrderId(77);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Order 77 not found"));
        _serviceMock.Verify(s => s.GetOrderItemsByOrderIdAsync(77), Times.Once);
    }

    #endregion
}
