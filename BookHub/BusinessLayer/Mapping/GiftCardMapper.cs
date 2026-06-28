using BusinessLayer.Dto.GiftCard;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping;

public static class GiftCardMapper
{
    public static GiftCardDto ToDto(GiftCard entity) =>
        EntityMapper.ToDto<GiftCard, GiftCardDto>(entity);

    public static IQueryable<GiftCardDto> ProjectToDto(IQueryable<GiftCard> query) =>
        EntityMapper.ProjectToDto<GiftCard, GiftCardDto>(query);
}