using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookHub.Mvc.Models.Admin;

public class AdminOrderFormViewModel
{
    public int Id { get; set; }
    
    
    public required string PaymentMethod { get; set; } 
    
    public IEnumerable<SelectListItem>? SelectedPaymentMethod { get; set; }
    
    public required string OrderState { get; set; }
    
    public IEnumerable<SelectListItem>? SelectedOrderState { get; set; }
    
    public bool Paid { get; set; }
    
    public float TotalPrice { get; set; }
    
    public List<int> SelectedOrderItemIds { get; set; } = new();
    
    public IEnumerable<SelectListItem> OrderItems { get; set; } = new List<SelectListItem>();
}