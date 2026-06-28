using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using BusinessLayer.Service;
using BusinessLayer.Dto.Author;
using DataAccessLayer;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Author;
using DataAccessLayer.Repository.Book;

namespace Tests.UnitTests.Servicies;

[TestFixture]
public class AuthorServiceTests
{
    private BookHubDbContext _ctx = null!;
    private AuthorRepository _authorRepo = null!;
    private Mock<IUnitOfWork> _uow = null!;
    private AuthorService _service = null!;

    [SetUp]
    public void Setup()
    {
        var opts = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase($"authors_{System.Guid.NewGuid()}")
            .Options;

        _ctx = new BookHubDbContext(opts);
        _authorRepo = new AuthorRepository(_ctx);

        _uow = new Mock<IUnitOfWork>();
        _uow.Setup(u => u.Authors).Returns(_authorRepo);

        var booksRepoMock = new Mock<IBookRepository>();
        booksRepoMock.Setup(r => r.Query()).Returns(_ctx.Books.AsQueryable());
        _uow.Setup(u => u.Books).Returns(booksRepoMock.Object);

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(ct => _ctx.SaveChangesAsync(ct));

        _uow.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<Func<Task<bool>>?>()))
            .Returns<Func<Task>, Func<Task<bool>>?>(
                async (op, verify) =>
                {
                    await op();
                    if (verify != null)
                    {
                        var ok = await verify();
                        if (!ok) throw new Exception("verification failed");
                    }
                });

        _service = new AuthorService(_uow.Object);
    }

    [TearDown]
    public void TearDown() => _ctx.Dispose();

    [Test]
    public async Task GetAuthorByIdAsync_ReturnsDto_WhenExists()
    {
        var a = new Author { Name = "Isaac Asimov" };
        _ctx.Authors.Add(a);
        await _ctx.SaveChangesAsync();

        var res = await _service.GetAuthorByIdAsync(a.Id);

        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Match(d => d.Name, _ => ""), Is.EqualTo("Isaac Asimov"));
    }

    [Test]
    public async Task GetAuthorByIdAsync_ReturnsError_WhenMissing()
    {
        var res = await _service.GetAuthorByIdAsync(9999);
        Assert.That(res.IsFaulted, Is.True);
        Assert.That(res.ToString(), Does.Contain("Author 9999 not found"));
    }

    [Test]
    public async Task GetAuthorsAsync_ReturnsMappedList_WhenOptionsNull()
    {
        _ctx.Authors.AddRange(
            new Author { Name = "A" },
            new Author { Name = "B" }
        );
        await _ctx.SaveChangesAsync();

        var res = await _service.GetAuthorsAsync(options: null);

        Assert.That(res.IsSuccess, Is.True);
        var list = res.Match(d => d, _ => new List<AuthorDto>());
        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.Select(x => x.Name), Does.Contain("A").And.Contain("B"));
    }

    [Test]
    public async Task AddAuthor_PersistsEntity_AndReturnsDto()
    {
        var req = new AddAuthorDto { Name = "New Author" };

        var res = await _service.AddAuthor(req);

        Assert.That(res.IsSuccess, Is.True);
        var dto = res.Match(d => d, _ => null!);
        Assert.That(dto.Name, Is.EqualTo("New Author"));

        var stored = await _ctx.Authors.FirstOrDefaultAsync(x => x.Name == "New Author");
        Assert.That(stored, Is.Not.Null);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateAuthor_ChangesName_AndSaves()
    {
        var a = new Author { Name = "Old" };
        _ctx.Authors.Add(a);
        await _ctx.SaveChangesAsync();

        var req = new UpdateAuthorDto { Name = "Updated" };
        var res = await _service.UpdateAuthor(a.Id, req);

        Assert.That(res.IsSuccess, Is.True);
        var dto = res.Match(d => d, _ => null!);
        Assert.That(dto.Name, Is.EqualTo("Updated"));

        var reloaded = await _ctx.Authors.FindAsync(a.Id);
        Assert.That(reloaded!.Name, Is.EqualTo("Updated"));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateAuthor_ReturnsError_WhenNotFound()
    {
        var req = new UpdateAuthorDto { Name = "X" };
        var res = await _service.UpdateAuthor(12345, req);

        Assert.That(res.IsFaulted, Is.True);
        Assert.That(res.ToString(), Does.Contain("Author 12345 not found"));
    }

    [Test]
    public async Task DeleteAuthorAsync_Deletes_WhenExists_AndNotUsed()
    {
        var a = new Author { Name = "To Delete" };
        _ctx.Authors.Add(a);
        await _ctx.SaveChangesAsync();

        _uow.Invocations.Clear();

        var res = await _service.DeleteAuthorAsync(a.Id);

        Assert.That(res.IsSuccess, Is.True);

        _ctx.ChangeTracker.Clear();

        var exists = await _ctx.Authors
            .AsNoTracking()
            .AnyAsync(x => x.Id == a.Id);

        Assert.That(exists, Is.False);

        var fromRepo = _authorRepo.Query().FirstOrDefault(x => x.Id == a.Id);
        Assert.That(fromRepo, Is.Null);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteAuthorAsync_ReturnsError_WhenMissing()
    {
        const int missingId = 999;

        _uow.Invocations.Clear();

        var res = await _service.DeleteAuthorAsync(missingId);

        Assert.That(res.IsFaulted, Is.True);
        Assert.That(res.ToString(), Does.Contain($"Author {missingId} not found"));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
