using BusinessLayer.Dto.Coupon;
using BusinessLayer.Service;
using DataAccessLayer;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Coupon;
using DataAccessLayer.Repository.GiftCard;
using Moq;

namespace Tests.UnitTests.Servicies;

[TestFixture]
public class CouponServiceTests
{
    private Mock<IUnitOfWork> _uowMock = null!;
    private Mock<ICouponRepository> _couponRepositoryMock = null!;
    private Mock<IGiftCardRepository> _giftCardRepositoryMock = null!;
    private CouponService _couponService = null!;

    [SetUp]
    public void SetUp()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _couponRepositoryMock = new Mock<ICouponRepository>();
        _giftCardRepositoryMock = new Mock<IGiftCardRepository>();

        _uowMock.Setup(u => u.Coupons).Returns(_couponRepositoryMock.Object);
        _uowMock.Setup(u => u.GiftCards).Returns(_giftCardRepositoryMock.Object);

        _uowMock.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), null))
                .Returns<Func<Task>, Func<Task<bool>>?>(async (op, _) => { await op(); });

        _couponService = new CouponService(_uowMock.Object);
    }

    #region CreateCoupon
    [Test]
    public async Task CreateCouponAsync_ShouldReturnOk_WhenGiftCardExists()
    {
        var dto = new CouponCreateDto { GiftCardId = 1 };
        var giftCard = new GiftCard { Id = 1 };
        _giftCardRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(giftCard);

        var result = await _couponService.CreateCouponAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        _couponRepositoryMock.Verify(r => r.AddRange(It.IsAny<IEnumerable<Coupon>>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task CreateCouponAsync_ShouldReturnError_WhenGiftCardNotFound()
    {
        var dto = new CouponCreateDto { GiftCardId = 99 };
        _giftCardRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((GiftCard?)null);

        var result = await _couponService.CreateCouponAsync(dto);

        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("GiftCard 99 not found"));
    }
    #endregion

    #region IsCouponValid
    [Test]
    public async Task IsCouponValidAsync_ShouldReturnTrue_WhenValid()
    {
        var code = "VALID-CODE";
        var now = DateTime.UtcNow;
        var coupons = new List<Coupon> 
        { 
            new()
            { 
                Code = code, 
                IsUsed = false, 
                GiftCard = new GiftCard { ValidFrom = now.AddDays(-1), ValidTo = now.AddDays(1) } 
            } 
        }.AsQueryable();

        _couponRepositoryMock.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Coupon>(coupons.Expression));

        var result = await _couponService.IsCouponValidAsync(code);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Match(m => m, e => false), Is.True);
    }

    [Test]
    public async Task IsCouponValidAsync_ShouldReturnError_WhenAlreadyUsed()
    {
        var code = "USED-CODE";
        var coupons = new List<Coupon> 
        { 
            new()
            { 
                Code = code, 
                IsUsed = true,
                GiftCard = new GiftCard { ValidFrom = DateTime.MinValue, ValidTo = DateTime.MaxValue } 
            } 
        }.AsQueryable();

        _couponRepositoryMock.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Coupon>(coupons.Expression));

        var result = await _couponService.IsCouponValidAsync(code);

        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("already used"));
    }
    #endregion

    #region DeleteCoupon
    [Test]
    public async Task DeleteCouponAsync_ShouldRemoveCoupon_WhenExists()
    {
        var code = "DELETE-ME";
        var coupon = new Coupon { Code = code };
        var coupons = new List<Coupon> { coupon }.AsQueryable();

        _couponRepositoryMock.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Coupon>(coupons.Expression));

        await _couponService.DeleteCouponAsync(code);

        _couponRepositoryMock.Verify(r => r.Remove(It.IsAny<Coupon>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.AtLeastOnce);
    }
    #endregion
}