using Microsoft.EntityFrameworkCore;

using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Publisher;

namespace Tests.UnitTests.Repositories;

[TestFixture]
public class PublisherRepositoryTests
{
    private BookHubDbContext _context = null!;
    private PublisherRepository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BookHubDbContext(options);
        _repo = new PublisherRepository(_context);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    #region Base Repository Tests
    [Test]
    public async Task Add_And_GetByIdAsync_ShouldWork()
    {
        var p = new Publisher { Name = "Penguin" };
        _repo.Add(p);
        await _context.SaveChangesAsync();

        var found = await _repo.GetByIdAsync(p.Id);
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Name, Is.EqualTo("Penguin"));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAll()
    {
        _context.Publishers.AddRange(
            new Publisher { Name = "A" },
            new Publisher { Name = "B" }
        );
        await _context.SaveChangesAsync();

        var all = await _repo.GetAllAsync();
        Assert.That(all.Count, Is.EqualTo(2));
        Assert.That(all.Select(x => x.Name), Does.Contain("A").And.Contain("B"));
    }

    [Test]
    public void Update_ShouldModifyEntity()
    {
        var p = new Publisher { Name = "Old" };
        _context.Publishers.Add(p);
        _context.SaveChanges();

        p.Name = "New";
        _repo.Update(p);
        _context.SaveChanges();

        var updated = _context.Publishers.Find(p.Id);
        Assert.That(updated!.Name, Is.EqualTo("New"));
    }

    [Test]
    public void Remove_ShouldSetIsRemoved()
    {
        var p = new Publisher { Name = "ToRemove" };
        _context.Publishers.Add(p);
        _context.SaveChanges();

        _repo.Remove(p);
        _context.SaveChanges();

        var removed = _context.Publishers.Find(p.Id);
        Assert.That(removed, Is.Not.Null);
        Assert.That(removed!.IsRemoved, Is.True);
    }

    [Test]
    public void Query_ShouldReturnQueryable_AndAllowFiltering()
    {
        _context.Publishers.AddRange(
            new Publisher { Name = "Alpha" },
            new Publisher { Name = "Beta" }
        );
        _context.SaveChanges();

        var q = _repo.Query().Where(x => x.Name.StartsWith("A"));
        Assert.That(q.Count(), Is.EqualTo(1));
        Assert.That(q.Single().Name, Is.EqualTo("Alpha"));
    }
    #endregion

    #region PublisherRepository Specific
    [Test]
    public async Task GetPublisherByIdAsync_ReturnsPublisher_WhenExists()
    {
        var p = new Publisher { Name = "HarperCollins" };
        _context.Publishers.Add(p);
        await _context.SaveChangesAsync();

        var fromRepo = await _repo.GetPublisherByIdAsync(p.Id);

        Assert.That(fromRepo, Is.Not.Null);
        Assert.That(fromRepo!.Name, Is.EqualTo("HarperCollins"));
    }

    [Test]
    public async Task GetPublisherByIdAsync_ReturnsNull_WhenNotFound()
    {
        var fromRepo = await _repo.GetPublisherByIdAsync(9999);
        Assert.That(fromRepo, Is.Null);
    }

    [Test]
    public async Task GetPublisherByIdAsync_UsesAsNoTracking()
    {
        var p = new Publisher { Name = "NoTrack Pub" };
        _context.Publishers.Add(p);
        await _context.SaveChangesAsync();

        var detached = await _repo.GetPublisherByIdAsync(p.Id);
        Assert.That(detached, Is.Not.Null);
        
        var state = _context.Entry(detached!).State;
        Assert.That(state, Is.EqualTo(EntityState.Detached));
    }
    #endregion
}
