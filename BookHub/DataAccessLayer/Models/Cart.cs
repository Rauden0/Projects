namespace DataAccessLayer.Models;

public class Cart : BaseEntity
{
    public int UserId { get; set; }
    public virtual User User { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public int? AppliedCouponId { get; set; }
    public virtual Coupon? AppliedCoupon { get; set; }
}