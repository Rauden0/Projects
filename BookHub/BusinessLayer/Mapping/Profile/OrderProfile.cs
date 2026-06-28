using BusinessLayer.Dto.Order;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping.Profile;

public class OrderMappingProfile : AutoMapper.Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.OrderItemsIds,
                opt => opt.MapFrom(s => s.OrderItems.Select(oi => oi.Id)))
            .ForMember(d => d.AppliedCouponId,
                opt => opt.MapFrom(s => s.CouponId ?? null));
    }
}