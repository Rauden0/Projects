using BusinessLayer.Dto.Order;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping;

public static class OrderItemMapper
{
    public static OrderItemDto ToDto(OrderItem entity) =>
        EntityMapper.ToDto<OrderItem, OrderItemDto>(entity);
    
    public static IQueryable<OrderItemDto> ProjectToDto(IQueryable<OrderItem> query) =>
        EntityMapper.ProjectToDto<OrderItem, OrderItemDto>(query);
}