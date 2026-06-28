using BusinessLayer.Dto.GiftCard;
using BusinessLayer.Service;
using DataAccessLayer;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Coupon;
using DataAccessLayer.Repository.GiftCard;
using Moq;

namespace Tests.UnitTests.Servicies;

[TestFixture]
public class GiftCardServiceTests
{
    private Mock<IUnitOfWork> _uowMock = null!;
    private Mock<IGiftCardRepository> _giftCardRepositoryMock = null!;
    private Mock<ICouponRepository> _couponRepositoryMock = null!;
    private GiftCardService _giftCardService = null!;

    [SetUp]
    public void SetUp()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _giftCardRepositoryMock = new Mock<IGiftCardRepository>();
        _couponRepositoryMock = new Mock<ICouponRepository>();

        _uowMock.Setup(u => u.GiftCards).Returns(_giftCardRepositoryMock.Object);
        _uowMock.Setup(u => u.Coupons).Returns(_couponRepositoryMock.Object);

        _uowMock.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), null))
                .Returns<Func<Task>, Func<Task<bool>>?>(async (op, _) => { await op(); });

        _giftCardService = new GiftCardService(_uowMock.Object);
    }

 
    #region GetGiftCard
    [Test]
    public async Task GetGiftCard_ShouldReturnResult_WhenExists()
    {
        var giftCard = new GiftCard { Id = 1, ReductionAmount = 50 };
        _giftCardRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(giftCard);

        var result = await _giftCardService.GetGiftCard(1);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Match(m => m.ReductionAmount, e => 0), Is.EqualTo(50));
    }

    [Test]
    public async Task GetGiftCard_ShouldReturnError_WhenNotFound()
    {
        _giftCardRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((GiftCard?)null);

        var result = await _giftCardService.GetGiftCard(99);

        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("GiftCard 99 not found"));
    }
    #endregion

    #region CreateGiftCard
    [Test]
    public async Task CreateGiftCard_ShouldReturnOk()
    {
        var dto = new GiftCardCreateDto { ReductionAmount = 100, ValidFrom = DateTime.Now, ValidTo = DateTime.Now.AddDays(10) };
        
        var result = await _giftCardService.CreateGiftCard(dto);

        Assert.That(result.IsSuccess, Is.True);
        _giftCardRepositoryMock.Verify(r => r.Add(It.IsAny<GiftCard>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
    #endregion

    #region DeleteGiftCard

    [Test]
    public async Task DeleteGiftCard_ShouldDoNothing_WhenNotFound()
    {
        _giftCardRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((GiftCard?)null);

        await _giftCardService.DeleteGiftCard(1);

        _giftCardRepositoryMock.Verify(r => r.Remove(It.IsAny<GiftCard>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Never);
    }
    
    [Test]
    public async Task DeleteGiftCard_WithAsyncEnumerable()
    {
        var giftCard = new GiftCard { Id = 1 };
        var coupons = new List<Coupon> { new() { Id = 10, GiftCardId = 1 } };
        
        _giftCardRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(giftCard);
        _couponRepositoryMock.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Coupon>(coupons));

        await _giftCardService.DeleteGiftCard(1);

        _couponRepositoryMock.Verify(r => r.Remove(It.IsAny<Coupon>()), Times.Once);
        _giftCardRepositoryMock.Verify(r => r.Remove(giftCard), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.AtLeastOnce);
    }
    #endregion
}