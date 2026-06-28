using BusinessLayer.Dto.Coupon;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;

namespace BusinessLayer.Service;

public interface ICouponService
{
    public Task<Result<List<CouponDto>>> CreateCouponAsync(CouponCreateDto couponCreateDto, int couponCount = 1);
    
    public Task<Result<bool>> IsCouponValidAsync(string code);
    
    public Task<Result<List<CouponDto>>> GetCoupons(ODataQueryOptions<CouponDto>? options);
    
    public Task<Result<CouponDto?>> GetCouponByCodeAsync(string code);
    
    public Task DeleteCouponAsync(string code);
    
}