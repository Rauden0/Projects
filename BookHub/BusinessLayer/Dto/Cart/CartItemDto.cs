namespace BusinessLayer.Dto.Cart;

public class CartItemDto
{
    public int BookId { get; set; }
    public int CartId { get; set; }
    public int Quantity { get; set; }
}