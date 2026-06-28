using DataAccessLayer.Enums;

namespace BusinessLayer.Dto.Order;

public class CreateOrderDto
{
    public int UserId { get; set; }
    public PaymentMethodEnum PaymentMethod { get; set; }
    public OrderStateEnum OrderState { get; set; } = OrderStateEnum.Preparing;
    public bool Paid { get; set; }
    public int? CouponId { get; set; }
    
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