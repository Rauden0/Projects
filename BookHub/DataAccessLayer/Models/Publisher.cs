namespace DataAccessLayer.Models;

public class Publisher : BaseEntity
{
    public string Name { get; set; } = default!;
    public virtual ICollection<Book>? Books { get; set; }
}