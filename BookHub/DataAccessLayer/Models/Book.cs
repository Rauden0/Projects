using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models;

public class Book : BaseEntity
{
    public virtual ICollection<Author> Authors { get; set; } = new List<Author>(); 
    
    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
    
    public int PublisherId { get; set; }
    
    public virtual Publisher Publisher { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    
    [Required]
    [MaxLength(250)]
    public string Description { get; set; }
    
    public float Price { get; set; }
    
    public int StockQuantity { get; set; }
    
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    
    public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public string? ImagePath { get; set; }
    
    public int? PrimaryGenreId { get; set; }
    
    public virtual Genre? PrimaryGenre { get; set; }

}