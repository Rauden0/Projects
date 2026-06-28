using BusinessLayer.Dto.Coupon;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHub.Mvc.Controller;

[Authorize(Roles = "Admin")]
public class AdminCouponController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ICouponService _couponService;

    public AdminCouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    public async Task<IActionResult> Index(string? q)
    {
        ViewData["q"] = q ?? "";

        List<CouponDto> list;

        if (string.IsNullOrWhiteSpace(q))
        {
            var res = await _couponService.GetCoupons(null);
            list = res.Match(
                succ => succ ?? new List<CouponDto>(),
                _ => new List<CouponDto>());
        }
        else
        {
            var res = await _couponService.GetCouponByCodeAsync(q.Trim());

            list = res.Match(
                succ => succ is null ? new List<CouponDto>() : new List<CouponDto> { succ },
                _ => new List<CouponDto>());
        }

        list = list.OrderByDescending(x => x.Id).ToList();

        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string code)
    {
        if (!string.IsNullOrWhiteSpace(code))
            await _couponService.DeleteCouponAsync(code.Trim());

        TempData["Ok"] = "Coupon deleted.";
        return RedirectToAction(nameof(Index));
    }
}