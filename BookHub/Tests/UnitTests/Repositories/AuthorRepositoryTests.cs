using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Author;

namespace Tests.UnitTests.Repositories;

[TestFixture]
public class AuthorRepositoryTests
{
    private BookHubDbContext _context = null!;
    private AuthorRepository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BookHubDbContext(options);
        _repo = new AuthorRepository(_context);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    #region Base Repository Tests
    [Test]
    public async Task Add_And_GetByIdAsync_ShouldWork()
    {
        var a = new Author { Name = "Frank Herbert" };
        _repo.Add(a);
        await _context.SaveChangesAsync();

        var found = await _repo.GetByIdAsync(a.Id);
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Name, Is.EqualTo("Frank Herbert"));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAll()
    {
        _context.Authors.AddRange(
            new Author { Name = "A" },
            new Author { Name = "B" }
        );
        await _context.SaveChangesAsync();

        var all = await _repo.GetAllAsync();
        Assert.That(all.Count, Is.EqualTo(2));
        Assert.That(all.Select(x => x.Name), Does.Contain("A").And.Contain("B"));
    }

    [Test]
    public void Update_ShouldModifyEntity()
    {
        var a = new Author { Name = "Old" };
        _context.Authors.Add(a);
        _context.SaveChanges();

        a.Name = "New";
        _repo.Update(a);
        _context.SaveChanges();

        var updated = _context.Authors.Find(a.Id);
        Assert.That(updated!.Name, Is.EqualTo("New"));
    }

    [Test]
    public void Remove_ShouldSetIsRemoved()
    {
        var a = new Author { Name = "ToRemove" };
        _context.Authors.Add(a);
        _context.SaveChanges();

        _repo.Remove(a);
        _context.SaveChanges();

        var removed = _context.Authors.Find(a.Id);
        Assert.That(removed, Is.Not.Null);
        Assert.That(removed!.IsRemoved, Is.True);
    }

    [Test]
    public void Query_ShouldReturnQueryable_AndAllowFiltering()
    {
        _context.Authors.AddRange(
            new Author { Name = "Alice" },
            new Author { Name = "Bob" }
        );
        _context.SaveChanges();

        var q = _repo.Query().Where(x => x.Name.StartsWith("A"));
        Assert.That(q.Count(), Is.EqualTo(1));
        Assert.That(q.Single().Name, Is.EqualTo("Alice"));
    }
    #endregion

    #region AuthorRepository Specific
    [Test]
    public async Task GetAuthorByIdAsync_ReturnsAuthor_WhenExists()
    {
        var a = new Author { Name = "Isaac Asimov" };
        _context.Authors.Add(a);
        await _context.SaveChangesAsync();

        var fromRepo = await _repo.GetAuthorByIdAsync(a.Id);

        Assert.That(fromRepo, Is.Not.Null);
        Assert.That(fromRepo!.Name, Is.EqualTo("Isaac Asimov"));
    }

    [Test]
    public async Task GetAuthorByIdAsync_ReturnsNull_WhenNotFound()
    {
        var fromRepo = await _repo.GetAuthorByIdAsync(9999);
        Assert.That(fromRepo, Is.Null);
    }

    [Test]
    public async Task GetAuthorByIdAsync_UsesAsNoTracking()
    {
        var a = new Author { Name = "NoTrack Author" };
        _context.Authors.Add(a);
        await _context.SaveChangesAsync();

        var detached = await _repo.GetAuthorByIdAsync(a.Id);
        Assert.That(detached, Is.Not.Null);

        // díky AsNoTracking by entity neměla být připojena ke kontextu
        var entryState = _context.Entry(detached!).State;
        Assert.That(entryState, Is.EqualTo(EntityState.Detached));
    }
    #endregion
}
