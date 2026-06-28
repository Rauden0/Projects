using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookHub.Mvc.Models.Admin;

public class AdminBookFormViewModel
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public float Price { get; set; }

    [Display(Name = "Authors")]
    public List<int> SelectedAuthorIds { get; set; } = new();

    [Display(Name = "Genres")]
    public List<int> SelectedGenreIds { get; set; } = new();

    [Display(Name = "Publisher")]
    public int PublisherId { get; set; }
    
    public int StockQuantity { get; set; }
    public IFormFile? Image { get; set; }
    
    public IEnumerable<SelectListItem> Authors { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Genres { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Publishers { get; set; } = new List<SelectListItem>();
}