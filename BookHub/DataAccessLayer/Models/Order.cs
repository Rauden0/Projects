using DataAccessLayer.Enums;

namespace DataAccessLayer.Models;

public class Order : BaseEntity
{
    public int UserId { get; set; }

    public virtual User User { get; set; }
    
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    
    public PaymentMethodEnum PaymentMethod { get; set; }

    public OrderStateEnum OrderState { get; set; }
    
    public bool Paid { get; set; } = false;
    
    public float TotalPrice { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public int? CouponId { get; set; }
    public virtual Coupon? Coupon { get; set; }
    // Checkout Info
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
        
    // Address
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

}