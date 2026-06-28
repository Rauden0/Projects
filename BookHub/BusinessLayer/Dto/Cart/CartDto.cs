namespace BusinessLayer.Dto.Cart;

public class CartDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<int> CartItemsIds { get; set; }
    public List<CartItemDto> CartItems { get; set; } = new();
    public decimal RawTotalPrice { get; set; } 
    public decimal DiscountAmount { get; set; }
    public decimal FinalTotalPrice => Math.Max(0, RawTotalPrice - DiscountAmount);
    
    public int? AppliedCouponId { get; set; }

}