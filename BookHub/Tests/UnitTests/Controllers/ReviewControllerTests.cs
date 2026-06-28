using BookHub.Controller;
using BusinessLayer.Dto.Review;
using BusinessLayer.Service;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Moq;
using DataAccessLayer.Enums;

namespace Tests.UnitTests.Controllers;

[TestFixture]
public class ReviewControllerTests
{
    private Mock<IReviewService> _serviceMock = null!;
    private ReviewController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IReviewService>();
        _controller = new ReviewController(_serviceMock.Object);
    }

    #region GetReviews
    [Test]
    public async Task GetReviews_ReturnsOk_WithList()
    {
        var list = new List<ReviewDto>
        {
            new() { Id = 1, BookId = 10, UserId = 7, Rating = (Rating)5, Comment = "Great" }
        };

        _serviceMock
            .Setup(s => s.GetReviews(It.IsAny<ODataQueryOptions<ReviewDto>>()))
            .ReturnsAsync(new Result<List<ReviewDto>>(list));

        var result = await _controller.GetReviews(null);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(list));
        _serviceMock.Verify(s => s.GetReviews(null), Times.Once);
    }

    [Test]
    public async Task GetReviews_ReturnsNotFound_OnError()
    {
        _serviceMock
            .Setup(s => s.GetReviews(It.IsAny<ODataQueryOptions<ReviewDto>>()))
            .ReturnsAsync(new Result<List<ReviewDto>>(new Exception("Error fetching reviews")));

        var result = await _controller.GetReviews(null);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Error fetching reviews"));
        _serviceMock.Verify(s => s.GetReviews(null), Times.Once);
    }
    #endregion

    #region GetReview
    [Test]
    public async Task GetReview_ReturnsOk_WhenFound()
    {
        var dto = new ReviewDto { Id = 2, BookId = 10, UserId = 7, Rating = (Rating)4, Comment = "Nice" };
        _serviceMock
            .Setup(s => s.GetReview(2))
            .ReturnsAsync(new Result<ReviewDto>(dto));

        var result = await _controller.GetReview(2);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(dto));
        _serviceMock.Verify(s => s.GetReview(2), Times.Once);
    }

    [Test]
    public async Task GetReview_ReturnsNotFound_WhenMissing()
    {
        _serviceMock
            .Setup(s => s.GetReview(42))
            .ReturnsAsync(new Result<ReviewDto>(new Exception("Review 42 not found")));

        var result = await _controller.GetReview(42);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Review 42 not found"));
        _serviceMock.Verify(s => s.GetReview(42), Times.Once);
    }
    #endregion

    #region AddReview
    [Test]
    public async Task AddReview_ReturnsOk_WhenCreated()
    {
        var req = new CreateReviewDto { BookId = 10, UserId = 7, Rating = (Rating)5, Comment = "Top" };
        var dto = new ReviewDto { Id = 99, BookId = 10, UserId = 7, Rating = (Rating)5, Comment = "Top" };

        _serviceMock
            .Setup(s => s.AddReview(req))
            .ReturnsAsync(new Result<ReviewDto>(dto));

        var result = await _controller.AddReview(req);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(((ReviewDto)ok!.Value).Id, Is.EqualTo(99));
        _serviceMock.Verify(s => s.AddReview(req), Times.Once);
    }

    [Test]
    public async Task AddReview_ReturnsNotFound_OnError()
    {
        var req = new CreateReviewDto { BookId = 10, UserId = 7, Rating = (Rating)77, Comment = "Invalid" }; // mimo rozsah

        _serviceMock
            .Setup(s => s.AddReview(req))
            .ReturnsAsync(new Result<ReviewDto>(new Exception("Rating must be between 1 and 5")));

        var result = await _controller.AddReview(req);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Rating must be between 1 and 5"));
        _serviceMock.Verify(s => s.AddReview(req), Times.Once);
    }
    #endregion

    #region UpdateReview
    [Test]
    public async Task UpdateReview_ReturnsOk_WhenUpdated()
    {
        var req = new UpdateReviewDto { Rating = (Rating)3, Comment = "Not Bad" };
        var dto = new ReviewDto { Id = 1, BookId = 10, UserId = 7, Rating = (Rating)3, Comment = "Not Bad" };

        _serviceMock
            .Setup(s => s.UpdateReview(1, req))
            .ReturnsAsync(new Result<ReviewDto>(dto));

        var result = await _controller.UpdateReview(1, req);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(((ReviewDto)ok!.Value).Rating, Is.EqualTo((Rating)3));
        _serviceMock.Verify(s => s.UpdateReview(1, req), Times.Once);
    }

    [Test]
    public async Task UpdateReview_ReturnsNotFound_OnError()
    {
        var req = new UpdateReviewDto { Rating = (Rating)2, Comment = "Not Good" };

        _serviceMock
            .Setup(s => s.UpdateReview(88, req))
            .ReturnsAsync(new Result<ReviewDto>(new Exception("Review 88 not found")));

        var result = await _controller.UpdateReview(88, req);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Review 88 not found"));
        _serviceMock.Verify(s => s.UpdateReview(88, req), Times.Once);
    }
    #endregion

    #region DeleteReview
    [Test]
    public async Task DeleteReview_CallsService_Once()
    {
        _serviceMock
            .Setup(s => s.DeleteReview(5))
            .Returns(Task.CompletedTask);

        await _controller.DeleteReview(5);

        _serviceMock.Verify(s => s.DeleteReview(5), Times.Once);
    }

    [Test]
    public void DeleteReview_BubblesException()
    {
        _serviceMock
            .Setup(s => s.DeleteReview(5))
            .ThrowsAsync(new Exception("Delete failed"));

        var ex = Assert.ThrowsAsync<Exception>(async () => await _controller.DeleteReview(5));
        Assert.That(ex!.Message, Is.EqualTo("Delete failed"));
        _serviceMock.Verify(s => s.DeleteReview(5), Times.Once);
    }
    #endregion
}
