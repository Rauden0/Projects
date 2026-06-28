using BusinessLayer.Dto.Order;
using BusinessLayer.Service;
using BusinessLayer.Facade;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace BookHub.Controller;

[ApiController]
[Route("/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ICheckoutFacade _checkoutFacade;

    public OrderController(IOrderService orderService, ICheckoutFacade checkoutFacade)
    {
        _orderService = orderService;
        _checkoutFacade = checkoutFacade;
    }

    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _orderService.GetOrder(id);
        return order.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
            );
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<OrderDto>))]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetOrders(ODataQueryOptions<OrderDto>? options)
    {
        var orders = await _orderService.GetOrders(options);
        return orders.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDto))]
    public async Task<IActionResult> AddOrder([FromBody] CreateOrderDto createOrderDto)
    {
        var order = await _orderService.AddOrder(createOrderDto);
        return Ok(order);
    }

    [HttpPut]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto updateOrderDto)
    {
        var order = await _orderService.UpdateOrder(id, updateOrderDto);
        return order.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }

    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        await _orderService.DeleteOrder(id);
        return NoContent();
    }

    [HttpPost]
    [Route("items")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddOrderItemToOrder([FromBody] OrderItemDto orderItemDto)
    {
        var order = await _orderService.AddOrderItemToOrder(orderItemDto);
        return order.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }

    [HttpDelete]
    [Route("items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveOrderItemFromOrder([FromBody] OrderItemDto orderItemDto)
    {
        await _orderService.RemoveOrderItemFromOrder(orderItemDto);
        return NoContent();
    }

    [HttpPost]
    [Route("{id}/items")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<OrderItemDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderItemInOrderByOrderId(int id)
    {
        var items = await _orderService.GetOrderItemsByOrderIdAsync(id);
        return items.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }
    
    [HttpPut]
    [Route("test-buy")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TestCreateOrder(int cartId,[FromBody] CreateOrderDto createOrderDto)
    {
        var order = await _checkoutFacade.GenerateOrderFromCartAsync(cartId, createOrderDto);
        return order.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }
}