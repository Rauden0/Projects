using BookHub.Controller;
using BusinessLayer.Dto.Publisher;
using BusinessLayer.Service;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Moq;

namespace Tests.UnitTests.Controllers;

[TestFixture]
public class PublishersControllerTests
{
    private Mock<IPublisherService> _serviceMock = null!;
    private PublishersController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IPublisherService>();
        _controller = new PublishersController(_serviceMock.Object);
    }

    #region GetPublisherAsync
    [Test]
    public async Task GetPublisherAsync_ReturnsOk_WhenFound()
    {
        var dto = new PublisherDto { Id = 1, Name = "Penguin" };

        _serviceMock
            .Setup(s => s.GetPublisherAsync(1))
            .ReturnsAsync(new Result<PublisherDto>(dto));

        var result = await _controller.GetPublisherAsync(1);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(dto));

        _serviceMock.Verify(s => s.GetPublisherAsync(1), Times.Once);
    }

    [Test]
    public async Task GetPublisherAsync_ReturnsNotFound_WhenMissing()
    {
        _serviceMock
            .Setup(s => s.GetPublisherAsync(42))
            .ReturnsAsync(new Result<PublisherDto>(new Exception("Publisher 42 not found")));

        var result = await _controller.GetPublisherAsync(42);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Publisher 42 not found"));

        _serviceMock.Verify(s => s.GetPublisherAsync(42), Times.Once);
    }
    #endregion

    #region GetPublishers
    [Test]
    public async Task GetPublishers_ReturnsOk_WithList()
    {
        var list = new List<PublisherDto> { new() { Id = 1, Name = "A" } };

        _serviceMock
            .Setup(s => s.GetPublishers(It.IsAny<ODataQueryOptions<PublisherDto>>()))
            .ReturnsAsync(new Result<List<PublisherDto>>(list));

        var result = await _controller.GetPublishers(null);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(list));

        _serviceMock.Verify(s => s.GetPublishers(It.IsAny<ODataQueryOptions<PublisherDto>>()), Times.Once);
    }

    [Test]
    public async Task GetPublishers_ReturnsNotFound_OnError()
    {
        _serviceMock
            .Setup(s => s.GetPublishers(It.IsAny<ODataQueryOptions<PublisherDto>>()))
            .ReturnsAsync(new Result<List<PublisherDto>>(new Exception("Error fetching publishers")));

        var result = await _controller.GetPublishers(null);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Error fetching publishers"));

        _serviceMock.Verify(s => s.GetPublishers(It.IsAny<ODataQueryOptions<PublisherDto>>()), Times.Once);
    }
    #endregion

    #region CreateAsync (POST)
    [Test]
    public async Task CreateAsync_ReturnsOk_WithResultDto()
    {
        var req = new CreatePublisherDto { Name = "NewPub" };
        var dto = new PublisherDto { Id = 7, Name = "NewPub" };

        _serviceMock
            .Setup(s => s.CreateAsync(req))
            .ReturnsAsync(new Result<PublisherDto>(dto));

        var result = await _controller.CreateAsync(req);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);

        // Controller vrací Ok(publisher) kde publisher je Result<PublisherDto>
        Assert.That(ok!.Value, Is.TypeOf<Result<PublisherDto>>());
        var inner = (Result<PublisherDto>)ok.Value!;

        Assert.That(inner.IsSuccess, Is.True);
        Assert.That(inner.Match(d => d.Name, _ => ""), Is.EqualTo("NewPub"));

        _serviceMock.Verify(s => s.CreateAsync(req), Times.Once);
    }

    [Test]
    public void CreateAsync_BubblesException_WhenServiceThrows()
    {
        var req = new CreatePublisherDto { Name = "" };

        _serviceMock
            .Setup(s => s.CreateAsync(req))
            .ThrowsAsync(new Exception("Name is required"));

        var ex = Assert.ThrowsAsync<Exception>(async () => await _controller.CreateAsync(req));
        Assert.That(ex!.Message, Is.EqualTo("Name is required"));

        _serviceMock.Verify(s => s.CreateAsync(req), Times.Once);
    }
    #endregion

    #region Update (PUT)
    [Test]
    public async Task Update_ReturnsOk_WhenUpdated()
    {
        var req = new UpdatePublisherDto { Name = "Updated" };
        var dto = new PublisherDto { Id = 5, Name = "Updated" };

        _serviceMock
            .Setup(s => s.UpdateAsync(5, req))
            .ReturnsAsync(new Result<PublisherDto>(dto));

        var result = await _controller.Update(5, req);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(((PublisherDto)ok!.Value!).Name, Is.EqualTo("Updated"));

        _serviceMock.Verify(s => s.UpdateAsync(5, req), Times.Once);
    }

    [Test]
    public async Task Update_ReturnsNotFound_OnError()
    {
        var req = new UpdatePublisherDto { Name = "X" };

        _serviceMock
            .Setup(s => s.UpdateAsync(99, req))
            .ReturnsAsync(new Result<PublisherDto>(new Exception("Publisher 99 not found")));

        var result = await _controller.Update(99, req);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Publisher 99 not found"));

        _serviceMock.Verify(s => s.UpdateAsync(99, req), Times.Once);
    }
    #endregion

    #region Delete
    [Test]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        _serviceMock
            .Setup(s => s.DeleteAsync(3))
            .ReturnsAsync(new Result<Unit>(Unit.Default));

        var result = await _controller.Delete(3);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        _serviceMock.Verify(s => s.DeleteAsync(3), Times.Once);
    }

    [Test]
    public async Task Delete_ReturnsNoContent_EvenWhenDeleteResultIsFaulted_BecauseControllerIgnoresResult()
    {
        _serviceMock
            .Setup(s => s.DeleteAsync(3))
            .ReturnsAsync(new Result<Unit>(new Exception("Publisher cannot be deleted because it is used by at least one book.")));

        var result = await _controller.Delete(3);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        _serviceMock.Verify(s => s.DeleteAsync(3), Times.Once);
    }

    [Test]
    public void Delete_BubblesException_WhenServiceThrows()
    {
        _serviceMock
            .Setup(s => s.DeleteAsync(3))
            .ThrowsAsync(new Exception("Delete failed"));

        var ex = Assert.ThrowsAsync<Exception>(async () => await _controller.Delete(3));
        Assert.That(ex!.Message, Is.EqualTo("Delete failed"));

        _serviceMock.Verify(s => s.DeleteAsync(3), Times.Once);
    }
    #endregion
}
