using BookHub.Controller;
using BusinessLayer.Dto.Author;
using BusinessLayer.Service;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Moq;

namespace Tests.UnitTests.Controllers;

[TestFixture]
public class AuthorsControllerTests
{
    private Mock<IAuthorService> _serviceMock = null!;
    private AuthorsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IAuthorService>();
        _controller = new AuthorsController(_serviceMock.Object);
    }

    #region GetAuthor
    [Test]
    public async Task GetAuthor_ReturnsOk_WhenFound()
    {
        var dto = new AuthorDto { Id = 1, Name = "Frank Herbert" };
        _serviceMock
            .Setup(s => s.GetAuthorByIdAsync(1))
            .ReturnsAsync(new Result<AuthorDto>(dto));

        var result = await _controller.GetAuthor(1);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(dto));
        _serviceMock.Verify(s => s.GetAuthorByIdAsync(1), Times.Once);
    }

    [Test]
    public async Task GetAuthor_ReturnsNotFound_WhenMissing()
    {
        _serviceMock
            .Setup(s => s.GetAuthorByIdAsync(42))
            .ReturnsAsync(new Result<AuthorDto>(new Exception("Author 42 not found")));

        var result = await _controller.GetAuthor(42);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Author 42 not found"));
        _serviceMock.Verify(s => s.GetAuthorByIdAsync(42), Times.Once);
    }
    #endregion

    #region GetAuthors
    [Test]
    public async Task GetAuthors_ReturnsOk_WithList()
    {
        var list = new List<AuthorDto> { new() { Id = 1, Name = "A" } };
        _serviceMock
            .Setup(s => s.GetAuthorsAsync(It.IsAny<ODataQueryOptions<AuthorDto>>()))
            .ReturnsAsync(new Result<List<AuthorDto>>(list));

        var result = await _controller.GetAuthors(null);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(list));
        _serviceMock.Verify(s => s.GetAuthorsAsync(null), Times.Once);
    }

    [Test]
    public async Task GetAuthors_ReturnsNotFound_OnError()
    {
        _serviceMock
            .Setup(s => s.GetAuthorsAsync(It.IsAny<ODataQueryOptions<AuthorDto>>()))
            .ReturnsAsync(new Result<List<AuthorDto>>(new Exception("Error fetching authors")));

        var result = await _controller.GetAuthors(null);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Error fetching authors"));
        _serviceMock.Verify(s => s.GetAuthorsAsync(null), Times.Once);
    }
    #endregion

    #region AddAuthor
    [Test]
    public async Task AddAuthor_ReturnsOk_WhenCreated()
    {
        var req = new AddAuthorDto { Name = "New Author" };
        var dto = new AuthorDto { Id = 7, Name = "New Author" };

        _serviceMock
            .Setup(s => s.AddAuthor(req))
            .ReturnsAsync(new Result<AuthorDto>(dto));

        var result = await _controller.AddAuthor(req);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(((AuthorDto)ok!.Value).Id, Is.EqualTo(7));
        _serviceMock.Verify(s => s.AddAuthor(req), Times.Once);
    }

    [Test]
    public async Task AddAuthor_ReturnsBadRequest_OnError()
    {
        var req = new AddAuthorDto { Name = "" }; // invalid
        _serviceMock
            .Setup(s => s.AddAuthor(req))
            .ReturnsAsync(new Result<AuthorDto>(new Exception("Name is required")));

        var result = await _controller.AddAuthor(req);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var bad = result as BadRequestObjectResult;
        Assert.That(bad!.Value, Is.EqualTo("Name is required"));
        _serviceMock.Verify(s => s.AddAuthor(req), Times.Once);
    }
    #endregion

    #region Update
    [Test]
    public async Task Update_ReturnsOk_WhenUpdated()
    {
        var req = new UpdateAuthorDto { Name = "Updated" };
        var dto = new AuthorDto { Id = 5, Name = "Updated" };

        _serviceMock
            .Setup(s => s.UpdateAuthor(5, req))
            .ReturnsAsync(new Result<AuthorDto>(dto));

        var result = await _controller.Update(5, req);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(((AuthorDto)ok!.Value).Name, Is.EqualTo("Updated"));
        _serviceMock.Verify(s => s.UpdateAuthor(5, req), Times.Once);
    }

    [Test]
    public async Task Update_ReturnsNotFound_OnError()
    {
        var req = new UpdateAuthorDto { Name = "X" };
        _serviceMock
            .Setup(s => s.UpdateAuthor(99, req))
            .ReturnsAsync(new Result<AuthorDto>(new Exception("Author 99 not found")));

        var result = await _controller.Update(99, req);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Author 99 not found"));
        _serviceMock.Verify(s => s.UpdateAuthor(99, req), Times.Once);
    }
    #endregion

    #region Delete
    [Test]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        _serviceMock
            .Setup(s => s.DeleteAuthorAsync(3))
            .ReturnsAsync(new Result<Unit>(Unit.Default));

        var result = await _controller.Delete(3);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        _serviceMock.Verify(s => s.DeleteAuthorAsync(3), Times.Once);
    }

    [Test]
    public async Task Delete_ReturnsConflict_WhenCannotDelete()
    {
        _serviceMock
            .Setup(s => s.DeleteAuthorAsync(3))
            .ReturnsAsync(new Result<Unit>(
                new Exception("Author cannot be deleted because it is used by at least one book.")
            ));

        var result = await _controller.Delete(3);

        Assert.That(result, Is.TypeOf<ConflictObjectResult>());
        var conflict = result as ConflictObjectResult;
        Assert.That(conflict!.Value, Is.EqualTo("Author cannot be deleted because it is used by at least one book."));

        _serviceMock.Verify(s => s.DeleteAuthorAsync(3), Times.Once);
    }

    #endregion
}
