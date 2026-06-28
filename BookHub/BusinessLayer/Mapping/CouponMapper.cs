using BusinessLayer.Dto.Coupon;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping;

public static class CouponMapper
{
    public static CouponDto ToDto(Coupon entity) =>
        EntityMapper.ToDto<Coupon, CouponDto>(entity);

    public static IQueryable<CouponDto> ProjectToDto(IQueryable<Coupon> query) =>
        EntityMapper.ProjectToDto<Coupon, CouponDto>(query);
}