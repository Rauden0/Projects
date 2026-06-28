using BusinessLayer.Dto.Book;
using BusinessLayer.Service;
using DataAccessLayer;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Book;
using DataAccessLayer.Repository.Order;
using DataAccessLayer.Repository.OrderItem;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace Tests.UnitTests.Servicies;

[TestFixture]
public class BookServiceTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<IBookRepository> _bookRepo = null!;
    private Mock<IOrderRepository> _orderRepo = null!;
    private Mock<IOrderItemRepository> _orderItemRepo = null!;
    private BookService _service = null!;
    private Mock<IImageService> _imageService = null!;
    private Mock<IWebHostEnvironment> _envMock = null!;

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _bookRepo = new Mock<IBookRepository>();
        _orderRepo = new Mock<IOrderRepository>();
        _orderItemRepo = new Mock<IOrderItemRepository>();
        _imageService = new Mock<IImageService>();
        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        
        _uow.Setup(u => u.Books).Returns(_bookRepo.Object);
        _uow.Setup(u => u.Orders).Returns(_orderRepo.Object);
        _uow.Setup(u => u.OrderItems).Returns(_orderItemRepo.Object);

        _service = new BookService(_uow.Object,_imageService.Object);
    }

    #region GetBook
    [Test]
    public async Task GetBook_ShouldReturnResult_WhenFound()
    {
        var book = new Book
        {
            Id = 1,
            Name = "Dune",
            Authors = new List<Author>(),
            Genres = new List<Genre>()
        };

        _bookRepo
            .Setup(r => r.GetBookByIdwithAuthorAndGenre(1))
            .ReturnsAsync(book);

        var result = await _service.GetBook(1);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Match(a => a.Name, ex => ex.Message), Is.EqualTo("Dune"));
    }

    [Test]
    public async Task GetBook_ShouldReturnError_WhenNotFound()
    {
        _bookRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Book?)null);

        var result = await _service.GetBook(99);

        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("Book 99 not found"));
    }
    #endregion

    #region AddBook
    [Test]
    public async Task AddBook_ShouldFail_WhenNoAuthors()
    {
        var req = new RequestBookDto { AuthorIds = new List<int> { 1 }, GenreIds = new List<int> { 1 } };
        _bookRepo.Setup(r => r.GetAuthorsByListIds(It.IsAny<List<int>>())).ReturnsAsync(new List<Author>());
        _bookRepo.Setup(r => r.GetGenresByListIds(It.IsAny<List<int>>())).ReturnsAsync(new List<Genre> { new() });

        var result = await _service.AddBook(req);

        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("Authors do not exist"));
    }

    [Test]
    public async Task AddBook_ShouldFail_WhenNoGenres()
    {
        var req = new RequestBookDto { AuthorIds = new List<int> { 1 }, GenreIds = new List<int> { 1 } };
        _bookRepo.Setup(r => r.GetAuthorsByListIds(It.IsAny<List<int>>())).ReturnsAsync(new List<Author> { new() });
        _bookRepo.Setup(r => r.GetGenresByListIds(It.IsAny<List<int>>())).ReturnsAsync(new List<Genre>());

        var result = await _service.AddBook(req);

        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("Genres do not exist"));
    }

    [Test]
    public async Task AddBook_ShouldSucceed_WhenValid()
    {
        var req = new RequestBookDto { Name = "New Book", AuthorIds = new List<int> { 1 }, GenreIds = new List<int> { 2 } };
        _bookRepo.Setup(r => r.GetAuthorsByListIds(req.AuthorIds)).ReturnsAsync(new List<Author> { new Author { Id = 1 } });
        _bookRepo.Setup(r => r.GetGenresByListIds(req.GenreIds)).ReturnsAsync(new List<Genre> { new Genre { Id = 2 } });

        var result = await _service.AddBook(req);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Match(a => a.Name, ex => ex.Message), Is.EqualTo("New Book"));
        _bookRepo.Verify(r => r.Add(It.IsAny<Book>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
    #endregion

    #region UpdateBook
    [Test]
    public async Task UpdateBook_ShouldFail_WhenBookNotFound()
    {
        var req = new RequestBookDto { Name = "X" };
        _bookRepo.Setup(r => r.GetBookByIdwithAuthorAndGenre(1)).ReturnsAsync((Book?)null);

        var result = await _service.UpdateBook(1, req);

        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("Book 1 not found"));
    }

    [Test]
    public async Task UpdateBook_ShouldFail_WhenNoAuthorsOrGenres()
    {
        var book = new Book { Id = 1 };
        var req = new RequestBookDto { AuthorIds = new List<int> { 1 }, GenreIds = new List<int> { 1 } };
        _bookRepo.Setup(r => r.GetBookByIdwithAuthorAndGenre(1)).ReturnsAsync(book);
        _bookRepo.Setup(r => r.GetAuthorsByListIds(req.AuthorIds)).ReturnsAsync(new List<Author>());
        _bookRepo.Setup(r => r.GetGenresByListIds(req.GenreIds)).ReturnsAsync(new List<Genre>());

        var result = await _service.UpdateBook(1, req);

        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("Authors not exist"));
    }

    [Test]
    public async Task UpdateBook_ShouldUpdatePrice_WhenDifferent()
    {
        var book = new Book { Id = 1, Price = 10 };
        var req = new RequestBookDto { AuthorIds = new List<int> { 1 }, GenreIds = new List<int> { 1 }, Price = 20 };
        _bookRepo.Setup(r => r.GetBookByIdwithAuthorAndGenre(1)).ReturnsAsync(book);
        _bookRepo.Setup(r => r.GetAuthorsByListIds(req.AuthorIds)).ReturnsAsync(new List<Author> { new Author { Id = 1 } });
        _bookRepo.Setup(r => r.GetGenresByListIds(req.GenreIds)).ReturnsAsync(new List<Genre> { new Genre { Id = 1 } });
        _orderItemRepo.Setup(o => o.GetOrderItemsByBookIdAsync(1)).ReturnsAsync(new List<OrderItem>());
        _orderRepo.Setup(o => o.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Order());

        var result = await _service.UpdateBook(1, req);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(book.Price, Is.EqualTo(20));
        _bookRepo.Verify(r => r.Update(book), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
    #endregion

    #region DeleteBook
    [Test]
    public async Task DeleteBook_ShouldRemoveBook_WhenExists()
    {
        var book = new Book { Id = 1 };
        _bookRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

        _uow.Setup(u => u.ExecuteInTransactionAsync(
                It.IsAny<Func<Task>>(),
                null
            ))
            .Returns<Func<Task>, Func<Task<bool>>?>(async (operation, verify) => await operation());

        await _service.DeleteBook(1);

        _bookRepo.Verify(r => r.Remove(book), Times.Once);
        _uow.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), null), Times.Once);
    }

    [Test]
    public async Task DeleteBook_ShouldDoNothing_WhenBookNull()
    {
        _uow.Setup(u => u.ExecuteInTransactionAsync(
                It.IsAny<Func<Task>>(), 
                null
            ))
            .Returns<Func<Task>, Func<Task<bool>>?>(
                async (operation, _) => await operation()
            );
        await _service.DeleteBook(1);

        _bookRepo.Verify(r => r.Remove(It.IsAny<Book>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
    #endregion
}