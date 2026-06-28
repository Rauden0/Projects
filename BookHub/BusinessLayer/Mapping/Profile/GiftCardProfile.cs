using BusinessLayer.Dto.GiftCard;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping.Profile;

public class GiftCardProfile : AutoMapper.Profile
{
    public GiftCardProfile()
    {
        CreateMap<GiftCard, GiftCardDto>();
    }
}