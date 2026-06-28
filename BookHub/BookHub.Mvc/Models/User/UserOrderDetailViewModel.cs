using BusinessLayer.Dto.Book;
using BusinessLayer.Dto.Order;

namespace BookHub.Mvc.Models.User;

public class UserOrderDetailViewModel
{
    public int Id { get; set; }
    public float TotalPrice { get; set; }
    public string OrderState { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public bool Paid { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public List<OrderItemDto> OrderItems { get; set; } = new();
    public List<BookDto> Books { get; set; } = new();
}