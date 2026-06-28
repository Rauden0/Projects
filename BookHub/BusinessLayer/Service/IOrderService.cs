using BusinessLayer.Dto.Order;
using DataAccessLayer.Models;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;

namespace BusinessLayer.Service;

public interface IOrderService
{
    Task<Result<OrderDto>> GetOrder(int id);
    Task<Result<List<OrderDto>>> GetOrders(ODataQueryOptions<OrderDto>? queryOptions);
    Task<Result<OrderDto>> AddOrder(CreateOrderDto createOrderDto);
    Task<Result<OrderDto>> UpdateOrder(int id, UpdateOrderDto updateOrderDto);
    Task<Result<OrderDto>> UpdateOrderTotalPriceUnsaved(int id, float totalPrice);
    Task DeleteOrder(int id);
    Task<Result<OrderDto>> AddOrderItemToOrder(OrderItemDto orderItemDto);
    Task<Result<OrderDto>> AddOrderItemToOrderUnsaved(OrderItemDto orderItemDto);
    Task RemoveOrderItemFromOrder(OrderItemDto orderItemDto);
    Task<Result<List<OrderItemDto>>> GetOrderItemsByOrderIdAsync(int orderId);
}