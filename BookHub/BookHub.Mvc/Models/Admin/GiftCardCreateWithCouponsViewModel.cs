using BusinessLayer.Dto.GiftCard;

namespace BookHub.Mvc.Models.Admin;

public class GiftCardCreateWithCouponsViewModel
{
    public GiftCardCreateDto GiftCard { get; set; } = new GiftCardCreateDto();

    public int CouponCount { get; set; } = 1;
}