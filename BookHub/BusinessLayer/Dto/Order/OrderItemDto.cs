namespace BusinessLayer.Dto.Order;

public class OrderItemDto
{
    public int BookId { get; set; }
    public int OrderId { get; set; }
    public int Quantity { get; set; }
}