using BusinessLayer.Dto.GiftCard;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;

namespace BusinessLayer.Service;

public interface IGiftCardService
{
    Task<Result<GiftCardDto>> CreateGiftCard(GiftCardCreateDto giftCardCreateDto);
    Task<Result<GiftCardDto>> GetGiftCard(int id);
    Task<Result<List<GiftCardDto>>> GetGiftCards(ODataQueryOptions<GiftCardDto>? queryOption);
    Task DeleteGiftCard(int id);
}