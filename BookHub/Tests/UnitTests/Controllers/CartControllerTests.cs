using BookHub.Controller;
using BusinessLayer.Dto.Cart;
using BusinessLayer.Service;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.UnitTests.Controllers;

[TestFixture]
public class CartControllerTests
{
    private Mock<ICartService> _serviceMock = null!;
    private CartController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<ICartService>();
        _controller = new CartController(_serviceMock.Object);
    }

    [Test]
    public async Task GetCart_ReturnsOk_WhenExists()
    {
        var dto = new CartDto { Id = 1 };
        _serviceMock.Setup(s => s.GetCart(1)).ReturnsAsync(new Result<CartDto>(dto));

        var result = await _controller.GetCart(1);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task ApplyCoupon_ReturnsOk_WhenValid()
    {
        var dto = new CartDto { Id = 1, AppliedCouponId = 5 };
        _serviceMock.Setup(s => s.ApplyCoupon(1, "CODE123")).ReturnsAsync(new Result<CartDto>(dto));

        var result = await _controller.ApplyCoupon(1, "CODE123");

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task ApplyCoupon_ReturnsBadRequest_OnFailure()
    {
        _serviceMock.Setup(s => s.ApplyCoupon(1, "INVALID")).ReturnsAsync(new Result<CartDto>(new Exception("Invalid code")));

        var result = await _controller.ApplyCoupon(1, "INVALID");

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }
}