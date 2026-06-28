using DataAccessLayer.Enums;

namespace DataAccessLayer.Models;

public class Review : BaseEntity
{
    public int UserId { get; set; }


    public virtual User User { get; set; }

    public int BookId { get; set; }

    public virtual Book Book { get; set; }

    public Rating Rating { get; set; }

    public string Comment { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}