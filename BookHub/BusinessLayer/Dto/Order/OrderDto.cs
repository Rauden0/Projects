using DataAccessLayer.Enums;

namespace BusinessLayer.Dto.Order;

public class OrderDto
{
    public int Id { get; set; }
    public PaymentMethodEnum PaymentMethod { get; set; }
    public OrderStateEnum OrderState { get; set; }
    public bool Paid { get; set; }
    public float TotalPrice { get; set; }
    public int UserId { get; set; }
    
    public int? AppliedCouponId { get; set; }
    public List<int> OrderItemsIds { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}