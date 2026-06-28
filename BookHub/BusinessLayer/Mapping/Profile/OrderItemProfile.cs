using BusinessLayer.Dto.Order;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping.Profile;

public class OrderItemProfile : AutoMapper.Profile
{
    public OrderItemProfile()
    {
        CreateMap<OrderItem, OrderItemDto>();
    }
}