using BusinessLayer.Dto.Order;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping;

public static class OrderMapper
{
    public static OrderDto ToDto(Order entity) =>
        EntityMapper.ToDto<Order, OrderDto>(entity);

    public static IQueryable<OrderDto> ProjectToDto(IQueryable<Order> query) =>
        EntityMapper.ProjectToDto<Order, OrderDto>(query);
}