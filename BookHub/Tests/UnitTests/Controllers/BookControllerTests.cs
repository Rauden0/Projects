using System.Security.Claims;
using BookHub.Controller;
using BusinessLayer.Dto.Book;
using BusinessLayer.Service;
using BusinessLayer.Service.Logging;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Moq;

namespace Tests.UnitTests.Controllers;

[TestFixture]
public class BookControllerTests
{
    private Mock<IBookService> _serviceMock = null!;
    private BookController _controller = null!;
    private Mock<IAuditLogService> _auditLogServiceMock = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IBookService>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _controller = new BookController(_serviceMock.Object, _auditLogServiceMock.Object);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, "test-user")
                    }, "mock"))
            }
        };

    }

    #region GetBook
    [Test]
    public async Task GetBook_ShouldReturnOk_WhenBookExists()
    {
        var dto = new BookDto { Id = 1, Name = "Dune" };
        _serviceMock.Setup(s => s.GetBook(1)).ReturnsAsync(new Result<BookDto>(dto));

        var result = await _controller.GetBook(1);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(dto));
    }

    [Test]
    public async Task GetBook_ShouldReturnNotFound_WhenBookDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetBook(42))
            .ReturnsAsync(new Result<BookDto>(new Exception("Book 42 not found")));

        var result = await _controller.GetBook(42);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Book 42 not found"));
    }
    #endregion

    #region GetBooks
    [Test]
    public async Task GetBooks_ShouldReturnOk_WithBooks()
    {
        var list = new List<BookDto> { new BookDto { Id = 1, Name = "Book1" } };
        _serviceMock.Setup(s => s.GetBooks(It.IsAny<ODataQueryOptions<BookDto>>(), false))
            .ReturnsAsync(new Result<List<BookDto>>(list));

        var result = await _controller.GetBooks(null);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(list));
    }

    [Test]
    public async Task GetBooks_ShouldReturnNotFound_OnException()
    {
        _serviceMock.Setup(s => s.GetBooks(It.IsAny<ODataQueryOptions<BookDto>>(), false))
            .ReturnsAsync(new Result<List<BookDto>>(new Exception("Error fetching books")));

        var result = await _controller.GetBooks(null);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Error fetching books"));
    }
    #endregion

    #region AddBook
    [Test]
    public async Task AddBook_ShouldReturnOk_WhenBookAdded()
    {
        var request = new RequestBookDto { Name = "New Book" };
        var dto = new BookDto { Id = 7, Name = "New Book" };
        _serviceMock.Setup(s => s.AddBook(request)).ReturnsAsync(new Result<BookDto>(dto));

        var result = await _controller.AddBook(request);
        var ok = result as OkObjectResult;

        Assert.That(ok, Is.Not.Null);
        Assert.That(((BookDto)ok!.Value).Id, Is.EqualTo(7));
    }

    [Test]
    public async Task AddBook_ShouldReturnBadRequest_OnException()
    {
        var request = new RequestBookDto { Name = "Invalid Book" };
        _serviceMock.Setup(s => s.AddBook(request))
            .ReturnsAsync(new Result<BookDto>(new Exception("Invalid authors")));

        var result = await _controller.AddBook(request);
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var bad = result as BadRequestObjectResult;
        Assert.That(bad!.Value, Is.EqualTo("Invalid authors"));
    }
    #endregion

    #region UpdateBook
    [Test]
    public async Task UpdateBook_ShouldReturnOk_WhenUpdated()
    {
        var request = new RequestBookDto { Name = "Updated Book" };
        var dto = new BookDto { Id = 1, Name = "Updated Book" };
        _serviceMock.Setup(s => s.UpdateBook(1, request)).ReturnsAsync(new Result<BookDto>(dto));

        var result = await _controller.UpdateBook(1, request);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(((BookDto)ok!.Value).Name, Is.EqualTo("Updated Book"));
        
        _auditLogServiceMock.Verify(s =>
            s.LogAsync(It.IsAny<string>(), "Book", "1", "Edit", "True"), Times.Once);
    }

    [Test]
    public async Task UpdateBook_ShouldReturnNotFound_WhenBookMissing()
    {
        var request = new RequestBookDto { Name = "Missing" };
        _serviceMock.Setup(s => s.UpdateBook(99, request))
            .ReturnsAsync(new Result<BookDto>(new Exception("Book 99 not found")));

        var result = await _controller.UpdateBook(99, request);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        
        _auditLogServiceMock.Verify(s =>
            s.LogAsync(It.IsAny<string>(), "Book", "99", "Edit", "False"), Times.Once);
    }
    #endregion

    #region DeleteBook
    [Test]
    public async Task DeleteBook_ShouldReturnNoContent_WhenSuccess()
    {
        _serviceMock.Setup(s => s.DeleteBook(3)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteBook(3);

        Assert.That(result, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteBook_ShouldHandleException()
    {
        _serviceMock.Setup(s => s.DeleteBook(5)).ThrowsAsync(new Exception("Delete failed"));

        try
        {
            await _controller.DeleteBook(5);
            Assert.Fail("Expected exception");
        }
        catch (Exception ex)
        {
            Assert.That(ex.Message, Is.EqualTo("Delete failed"));
        }
    }
    #endregion
}
