namespace BusinessLayer.Dto.Coupon;

public class CouponDto
{
    public int Id { get; set; }

    public string Code { get; set; } 
    public bool IsUsed { get; set; }
    public int? OrderId { get; set; }
    public int? GiftCardId { get; set; }
    
}