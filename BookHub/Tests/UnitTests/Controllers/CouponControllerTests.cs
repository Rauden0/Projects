using BookHub.Controller;
using BusinessLayer.Dto.Coupon;
using BusinessLayer.Service;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.UnitTests.Controllers;

[TestFixture]
public class CouponControllerTests
{
    private Mock<ICouponService> _serviceMock = null!;
    private CouponController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<ICouponService>();
        _controller = new CouponController(_serviceMock.Object);
    }

    [Test]
    public async Task IsCouponValid_ReturnsOkWithBool_WhenSuccess()
    {
        _serviceMock.Setup(s => s.IsCouponValidAsync("VALID")).ReturnsAsync(new Result<bool>(true));

        var result = await _controller.IsCouponValid("VALID");

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(true));
    }

    [Test]
    public async Task CreateCoupon_ReturnsBadRequest_WhenServiceFails()
    {
        var req = new CouponCreateDto { GiftCardId = 1 };
        _serviceMock.Setup(s => s.CreateCouponAsync(req, 1)).ReturnsAsync(new Result<List<CouponDto>>(new Exception("Error")));

        var result = await _controller.CreateCoupon(req);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }
}