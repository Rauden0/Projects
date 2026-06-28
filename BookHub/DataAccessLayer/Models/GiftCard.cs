namespace DataAccessLayer.Models;

public class GiftCard : BaseEntity
{
    public decimal ReductionAmount { get; set; } // e.g. 200 CZK
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public virtual ICollection<Coupon> Coupons { get; set; } = [];
}