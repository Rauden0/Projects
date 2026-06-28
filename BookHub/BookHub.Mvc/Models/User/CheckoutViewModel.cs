using System.ComponentModel.DataAnnotations;
using BusinessLayer.Dto.Book;
using DataAccessLayer.Enums;
using BusinessLayer.Dto.Cart;

namespace BookHub.Mvc.Models.User;

public class CheckoutViewModel
{
    public int CartId { get; set; }
    
    [Required(ErrorMessage = "Musíte vybrat platební metodu.")]
    public PaymentMethodEnum? PaymentMethod { get; set; }

    public string? CouponCode { get; set; }
    
    public CartDto? Cart { get; set; }
    
    public List<CartItemDto> CartItems { get; set; } = new();
    
    public List<BookDto> Books { get; set; } = new();
    
    [Required]
    [MaxLength(50)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    // Address
    [Required]
    [MaxLength(100)]
    public string Street { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string City { get; set; } = string.Empty;
    [Required]
    [MaxLength(20)]
    [Display(Name = "Postal Code")]
    public string PostalCode { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string Country { get; set; } = string.Empty;
    
}