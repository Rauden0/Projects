namespace DataAccessLayer.Models;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    
    public virtual Order Order { get; set; }
    
    public int BookId { get; set; }
    
    public virtual Book Book { get; set; }
    
    public int Quantity { get; set; }
    
    public float Price { get; set; }
    
    public DateTime AddedAt { get; set; } = DateTime.Now;
}