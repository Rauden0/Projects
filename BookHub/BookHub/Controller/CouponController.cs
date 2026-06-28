using BusinessLayer.Service;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace BookHub.Controller;

[ApiController]
[Route("/coupons")]
public class CouponController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BusinessLayer.Dto.Coupon.CouponDto>))]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetCoupons([FromQuery] ODataQueryOptions<BusinessLayer.Dto.Coupon.CouponDto>? options)
    {
        var coupons = await _couponService.GetCoupons(options);
        return coupons.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BusinessLayer.Dto.Coupon.CouponDto))]
    public async Task<IActionResult> CreateCoupon([FromBody] BusinessLayer.Dto.Coupon.CouponCreateDto couponCreateDto)
    {
        var coupon = await _couponService.CreateCouponAsync(couponCreateDto);
        return coupon.Match<IActionResult>(
            Ok,
            ex => BadRequest(ex.Message)
        );
    }

    [HttpGet]
    [Route("{code}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BusinessLayer.Dto.Coupon.CouponDto>))]
    public async Task<IActionResult> GetCouponByCode([FromQuery] string code)
    {
        var coupons = await _couponService.GetCouponByCodeAsync(code);
        return coupons.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }

    [HttpGet]
    [Route("validate/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public async Task<IActionResult> IsCouponValid([FromRoute] string code)
    {
        var isValidResult = await _couponService.IsCouponValidAsync(code);
        return isValidResult.Match<IActionResult>(
            isValid => Ok(isValid),
            ex => NotFound(ex.Message)
        );
    }
    
    [HttpDelete]
    [Route("{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(bool))]
    public async Task<IActionResult> DeleteCoupon([FromRoute] string code)
    {
        await _couponService.DeleteCouponAsync(code);
        return NoContent();
    }
    
}