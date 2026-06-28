using BookHub.Controller;
using BusinessLayer.Dto.GiftCard;
using BusinessLayer.Service;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.UnitTests.Controllers;

[TestFixture]
public class GiftCardControllerTests
{
    private Mock<IGiftCardService> _serviceMock = null!;
    private GiftCardController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IGiftCardService>();
        _controller = new GiftCardController(_serviceMock.Object);
    }

    [Test]
    public async Task GetGiftCard_ReturnsNotFound_WhenMissing()
    {
        _serviceMock.Setup(s => s.GetGiftCard(99)).ReturnsAsync(new Result<GiftCardDto>(new Exception("Not found")));

        var result = await _controller.GetGiftCardByCode(99);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteGiftCard_ReturnsNoContent()
    {
        var result = await _controller.DeleteGiftCard(1);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        _serviceMock.Verify(s => s.DeleteGiftCard(1), Times.Once);
    }
}