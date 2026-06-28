using BusinessLayer.Dto.Coupon;

namespace BusinessLayer.Dto.GiftCard;

public class GiftCardCreateDto
{
    public decimal ReductionAmount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    
}