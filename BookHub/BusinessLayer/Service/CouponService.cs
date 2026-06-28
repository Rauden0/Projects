using BusinessLayer.Dto.Coupon;
using BusinessLayer.Extension;
using BusinessLayer.Mapping;
using DataAccessLayer;
using DataAccessLayer.Models;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace BusinessLayer.Service;

public class CouponService : ICouponService
{
    private readonly IUnitOfWork _uow;

    public CouponService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<List<CouponDto>>> CreateCouponAsync(CouponCreateDto dto, int couponCount = 1)
    {
        var giftCard = await _uow.GiftCards.GetByIdAsync(dto.GiftCardId);
        if (giftCard is null)
        {
            return new Result<List<CouponDto>>(new Exception($"GiftCard {dto.GiftCardId} not found"));
        }

        var coupons = new List<Coupon>();
        for (int i = 0; i < couponCount; i++)
        {
            coupons.Add(new Coupon
            {
                Code = Guid.NewGuid().ToString(),
                GiftCardId = dto.GiftCardId,
                IsUsed = false
            });
        }

        _uow.Coupons.AddRange(coupons);
        await _uow.SaveChangesAsync();
        return coupons.Select(CouponMapper.ToDto).ToList();
    }

    public async Task<Result<bool>> IsCouponValidAsync(string code)
    {
        var now = DateTime.UtcNow;

        var couponStatus = await _uow.Coupons.Query()
            .Where(c => c.Code == code)
            .Select(c => new
            {
                c.IsUsed,
                IsValidDate = now >= c.GiftCard.ValidFrom && now <= c.GiftCard.ValidTo
            })
            .FirstOrDefaultAsync();

        return couponStatus switch
        {
            null => new Result<bool>(new Exception("Code does not exist")),
            { IsUsed: true } => new Result<bool>(new Exception("Code already used")),
            { IsValidDate: false } => new Result<bool>(new Exception("Code expired or not yet valid")),
            _ => true
        };
    }

    public async Task<Result<List<CouponDto>>> GetCoupons(ODataQueryOptions<CouponDto>? options)
    {
        return await CouponMapper.ProjectToDto(_uow.Coupons.Query()).ApplyIfNotNull(options).ToListAsync();
    }

    public async Task<Result<CouponDto?>> GetCouponByCodeAsync(string code)
    {
        return await CouponMapper.ProjectToDto(_uow.Coupons.Query()
            .Where(c => c.Code == code)).FirstOrDefaultAsync();
    }

    public async Task DeleteCouponAsync(string code)
    {
        await _uow.ExecuteInTransactionAsync(async () =>
        {
            var coupon = await _uow.Coupons.Query().FirstOrDefaultAsync(c => c.Code == code);
            if (coupon != null)
            {
                _uow.Coupons.Remove(coupon);
                await _uow.SaveChangesAsync();
            }
        });
    }
}