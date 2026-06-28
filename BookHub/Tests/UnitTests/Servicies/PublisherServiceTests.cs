using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

using BusinessLayer.Service;
using BusinessLayer.Dto.Publisher;
using DataAccessLayer;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Publisher;
using DataAccessLayer.Repository.Book;

namespace Tests.UnitTests.Servicies;

[TestFixture]
public class PublisherServiceTests
{
    private BookHubDbContext _ctx = null!;
    private PublisherRepository _publisherRepo = null!;
    private BookRepository _bookRepo = null!;
    private Mock<IUnitOfWork> _uow = null!;
    private PublisherService _service = null!;

    [SetUp]
    public void Setup()
    {
        var opts = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase($"publishers_{System.Guid.NewGuid()}")
            .Options;

        _ctx = new BookHubDbContext(opts);

        _publisherRepo = new PublisherRepository(_ctx);
        _bookRepo = new BookRepository(_ctx);

        _uow = new Mock<IUnitOfWork>();

        _uow.Setup(u => u.Publishers).Returns(_publisherRepo);
        _uow.Setup(u => u.Books).Returns(_bookRepo);

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(ct => _ctx.SaveChangesAsync(ct));

        _service = new PublisherService(_uow.Object);
    }

    [TearDown]
    public void TearDown() => _ctx.Dispose();

    // -------- GetPublisherAsync --------

    [Test]
    public async Task GetPublisherAsync_ReturnsDto_WhenExists()
    {
        var p = new Publisher { Name = "Penguin" };
        _ctx.Publishers.Add(p);
        await _ctx.SaveChangesAsync();

        var res = await _service.GetPublisherAsync(p.Id);

        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Match(d => d.Name, _ => ""), Is.EqualTo("Penguin"));
    }

    [Test]
    public async Task GetPublisherAsync_ReturnsError_WhenMissing()
    {
        var res = await _service.GetPublisherAsync(9999);

        Assert.That(res.IsFaulted, Is.True);
        Assert.That(res.ToString(), Does.Contain("Publisher 9999 not found"));
    }

    // -------- GetPublishers --------

    [Test]
    public async Task GetPublishers_ReturnsList_WhenOptionsNull()
    {
        _ctx.Publishers.AddRange(new Publisher { Name = "A" }, new Publisher { Name = "B" });
        await _ctx.SaveChangesAsync();

        var res = await _service.GetPublishers(options: null);

        Assert.That(res.IsSuccess, Is.True);
        var list = res.Match(d => d, _ => new List<PublisherDto>());
        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.Select(x => x.Name), Does.Contain("A").And.Contain("B"));
    }

    // -------- CreateAsync --------

    [Test]
    public async Task CreateAsync_Persists_AndReturnsDto()
    {
        var req = new CreatePublisherDto { Name = "NewPub" };

        _uow.Invocations.Clear();

        var res = await _service.CreateAsync(req);

        Assert.That(res.IsSuccess, Is.True);
        var dto = res.Match(d => d, _ => null!);
        Assert.That(dto.Name, Is.EqualTo("NewPub"));

        var stored = await _ctx.Publishers.FirstOrDefaultAsync(x => x.Name == "NewPub");
        Assert.That(stored, Is.Not.Null);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // -------- UpdateAsync --------

    [Test]
    public async Task UpdateAsync_ChangesName_AndSaves()
    {
        var p = new Publisher { Name = "Old" };
        _ctx.Publishers.Add(p);
        await _ctx.SaveChangesAsync();

        var req = new UpdatePublisherDto { Name = "Updated" };

        _uow.Invocations.Clear();

        var res = await _service.UpdateAsync(p.Id, req);

        Assert.That(res.IsSuccess, Is.True);
        var dto = res.Match(d => d, _ => null!);
        Assert.That(dto.Name, Is.EqualTo("Updated"));

        var reloaded = await _ctx.Publishers.FindAsync(p.Id);
        Assert.That(reloaded!.Name, Is.EqualTo("Updated"));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_ReturnsError_WhenNotFound()
    {
        var req = new UpdatePublisherDto { Name = "X" };

        _uow.Invocations.Clear();

        var res = await _service.UpdateAsync(12345, req);

        Assert.That(res.IsFaulted, Is.True);
        Assert.That(res.ToString(), Does.Contain("Publisher 12345 not found"));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // -------- DeleteAsync (safe delete) --------

    [Test]
    public async Task DeleteAsync_Deletes_WhenExists_AndNotUsed()
    {
        var p = new Publisher { Name = "ToDelete" };
        _ctx.Publishers.Add(p);
        await _ctx.SaveChangesAsync();

        _uow.Invocations.Clear();

        var res = await _service.DeleteAsync(p.Id);

        Assert.That(res.IsSuccess, Is.True);
        
        _ctx.ChangeTracker.Clear();

        var existsInDb = await _ctx.Publishers
            .AsNoTracking()
            .AnyAsync(x => x.Id == p.Id);

        Assert.That(existsInDb, Is.False, "publisher should not exist in DB after delete");

        var fromRepo = _publisherRepo.Query().FirstOrDefault(x => x.Id == p.Id);
        Assert.That(fromRepo, Is.Null);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Test]
    public async Task DeleteAsync_ReturnsError_WhenMissing()
    {
        const int missingId = 999;

        _uow.Invocations.Clear();

        var res = await _service.DeleteAsync(missingId);

        Assert.That(res.IsFaulted, Is.True);
        Assert.That(res.ToString(), Does.Contain($"Publisher {missingId} not found"));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task DeleteAsync_ReturnsError_WhenUsedByAtLeastOneBook()
    {
        var p = new Publisher { Name = "UsedPublisher" };
        _ctx.Publishers.Add(p);
        await _ctx.SaveChangesAsync();
        
        _ctx.Books.Add(new Book
        {
            Name = "Book",
            Description = "Desc",
            PublisherId = p.Id
        });
        await _ctx.SaveChangesAsync();

        _uow.Invocations.Clear();

        var res = await _service.DeleteAsync(p.Id);

        Assert.That(res.IsFaulted, Is.True);
        Assert.That(res.ToString(), Does.Contain("Publisher cannot be deleted because it is used by at least one book."));

        var stillThere = await _ctx.Publishers.FindAsync(p.Id);
        Assert.That(stillThere, Is.Not.Null);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
