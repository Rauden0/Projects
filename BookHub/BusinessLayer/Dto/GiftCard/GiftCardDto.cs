namespace BusinessLayer.Dto.GiftCard;

public class GiftCardDto
{
    public int Id { get; set; }
    public decimal ReductionAmount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    

}