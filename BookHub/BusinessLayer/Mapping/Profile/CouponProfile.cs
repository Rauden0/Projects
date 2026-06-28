using BusinessLayer.Dto.Coupon;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping.Profile;

public class CouponProfile : AutoMapper.Profile
{
    public CouponProfile()
    {
        CreateMap<Coupon, CouponDto>();
    }
}