using BusinessLayer.Dto.Coupon;
using BusinessLayer.Dto.GiftCard;
using BusinessLayer.Service;
using BookHub.Mvc.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHub.Mvc.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class AdminGiftCardController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly IGiftCardService _giftCardService;
    private readonly ICouponService _couponService;

    public AdminGiftCardController(IGiftCardService giftCardService, ICouponService couponService)
    {
        _giftCardService = giftCardService;
        _couponService = couponService;
    }

    public async Task<IActionResult> Index()
    {
        var res = await _giftCardService.GetGiftCards(null);
        var list = res.Match(
            succ => succ ?? new List<GiftCardDto>(),
            _ => new List<GiftCardDto>());

        return View(list);
    }

    public IActionResult Create()
    {
        var vm = new GiftCardCreateWithCouponsViewModel
        {
            GiftCard = new GiftCardCreateDto
            {
                ReductionAmount = 200,
                ValidFrom = DateTime.UtcNow.Date,
                ValidTo = DateTime.UtcNow.Date.AddMonths(1)
            },
            CouponCount = 5
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GiftCardCreateWithCouponsViewModel vm)
    {
        vm.GiftCard ??= new GiftCardCreateDto();

        if (!ModelState.IsValid)
        {
            TempData["Err"] = "Form contains errors. Please fix validation messages below.";
            return View(vm);
        }

        if (vm.GiftCard.ValidTo < vm.GiftCard.ValidFrom)
        {
            ModelState.AddModelError("GiftCard.ValidTo", "ValidTo must be >= ValidFrom.");
            return View(vm);
        }

        if (vm.CouponCount < 1)
        {
            ModelState.AddModelError(nameof(vm.CouponCount), "CouponCount must be >= 1.");
            return View(vm);
        }

        var createdGift = await _giftCardService.CreateGiftCard(vm.GiftCard);

        return await createdGift.Match<Task<IActionResult>>(
            async gift =>
            {
                var cRes = await _couponService.CreateCouponAsync(new CouponCreateDto
                    {
                        GiftCardId = gift.Id
                    }, vm.CouponCount
                );
                var ok = cRes.Match(
                    _ => true,
                    ex =>
                    {
                        ModelState.AddModelError("", ex.Message);
                        return false;
                    });

                if (!ok)
                    return View(vm);


                TempData["Ok"] = $"Gift card #{gift.Id} created. Generated {vm.CouponCount} coupons.";
                return RedirectToAction(nameof(Details), new { id = gift.Id });
            },
            ex =>
            {
                ModelState.AddModelError("", ex.Message);
                return Task.FromResult<IActionResult>(View(vm));
            });
    }

    public async Task<IActionResult> Details(int id)
    {
        var res = await _giftCardService.GetGiftCard(id);
        return res.Match<IActionResult>(
            succ => View(succ),
            _ => NotFound());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _giftCardService.DeleteGiftCard(id);
        TempData["Ok"] = $"Gift card #{id} deleted.";
        return RedirectToAction(nameof(Index));
    }
}