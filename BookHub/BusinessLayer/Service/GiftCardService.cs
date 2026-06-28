using BusinessLayer.Extension;
using BusinessLayer.Mapping;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service;

using Dto.GiftCard;
using DataAccessLayer;
using DataAccessLayer.Models;
using LanguageExt.Common;

public class GiftCardService : IGiftCardService
{
    private readonly IUnitOfWork _uow;

    public GiftCardService(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<GiftCardDto>> CreateGiftCard(GiftCardCreateDto dto)
    {
        var giftCard = new GiftCard
        {
            ReductionAmount = dto.ReductionAmount,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo
        };

        _uow.GiftCards.Add(giftCard);
        await _uow.SaveChangesAsync();
        return GiftCardMapper.ToDto(giftCard);
    }
    
    public async Task<Result<GiftCardDto>> GetGiftCard(int id)
    {
        var giftCard = await _uow.GiftCards.GetByIdAsync(id);
        return giftCard is not null
            ? GiftCardMapper.ToDto(giftCard)
            : new Result<GiftCardDto>(new Exception($"GiftCard {id} not found"));
    }
    
    public async Task<Result<List<GiftCardDto>>> GetGiftCards(ODataQueryOptions<GiftCardDto>? options)
    {
        return await GiftCardMapper.ProjectToDto(_uow.GiftCards.Query()).ApplyIfNotNull(options)
            .ToListAsync();
    }
    
    public async Task DeleteGiftCard(int id)
    {
        var giftCard = await _uow.GiftCards.GetByIdAsync(id);
        if (giftCard is not null)
        {
            _uow.ExecuteInTransactionAsync(
                async () =>
                {
                    var couponsWithGiftCard = _uow.Coupons.Query().Where(o => o.GiftCardId == id);
                    await foreach (var coupon in couponsWithGiftCard.AsAsyncEnumerable())
                    {
                        _uow.Coupons.Remove(coupon);
                    }
                    _uow.GiftCards.Remove(giftCard);
                }).Wait();
          
            await _uow.SaveChangesAsync();
        }
    }
    

}