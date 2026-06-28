using DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Review;

namespace Tests.UnitTests.Repositories;

[TestFixture]
public class ReviewRepositoryTests
{
    private BookHubDbContext _context = null!;
    private ReviewRepository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BookHubDbContext(options);
        _repo = new ReviewRepository(_context);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    #region Base Repository Tests
    [Test]
    public async Task Add_And_GetByIdAsync_ShouldWork()
    {
        var user = new User { Email = "alice@example.com", PasswordHash = "x" };
        var book = new Book { Name = "Dune", Description = "Sci-fi epic." };
        _context.Users.Add(user);
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var review = new Review { BookId = book.Id, UserId = user.Id, Rating = (Rating)5, Comment = "Great!" };
        _repo.Add(review);
        await _context.SaveChangesAsync();

        var found = await _repo.GetByIdAsync(review.Id);
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Rating, Is.EqualTo((Rating)5));
        Assert.That(found.Comment, Is.EqualTo("Great!"));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllReviews()
    {
        var u = new User { Email = "bob@example.com", PasswordHash = "x" };
        var b = new Book { Name = "Foundation", Description = "Classic." };
        _context.AddRange(u, b);
        await _context.SaveChangesAsync();

        _context.Reviews.AddRange(
            new Review { BookId = b.Id, UserId = u.Id, Rating = (Rating)4, Comment = "Nice" },
            new Review { BookId = b.Id, UserId = u.Id, Rating = (Rating)2, Comment = "Meh" }
        );
        await _context.SaveChangesAsync();

        var all = await _repo.GetAllAsync();
        Assert.That(all.Count, Is.EqualTo(2));
        Assert.That(all.Select(r => r.Comment), Does.Contain("Nice").And.Contain("Meh"));
    }

    [Test]
    public void Update_ShouldModifyEntity()
    {
        var u = new User { Email = "carol@example.com", PasswordHash = "x" };
        var b = new Book { Name = "Neuromancer", Description = "Cyberpunk." };
        _context.AddRange(u, b);
        _context.SaveChanges();

        var r = new Review { BookId = b.Id, UserId = u.Id, Rating = (Rating)2, Comment = "ok" };
        _context.Reviews.Add(r);
        _context.SaveChanges();

        r.Rating = (Rating)5;
        r.Comment = "excellent";
        _repo.Update(r);
        _context.SaveChanges();

        var updated = _context.Reviews.Find(r.Id);
        Assert.That(updated!.Rating, Is.EqualTo((Rating)5));
        Assert.That(updated.Comment, Is.EqualTo("excellent"));
    }

    [Test]
    public void Remove_ShouldSetIsRemoved()
    {
        var u = new User { Email = "dave@example.com", PasswordHash = "x" };
        var b = new Book { Name = "Snow Crash", Description = "Metaverse." };
        _context.AddRange(u, b);
        _context.SaveChanges();

        var r = new Review { BookId = b.Id, UserId = u.Id, Rating = (Rating)4, Comment = "solid" };
        _context.Reviews.Add(r);
        _context.SaveChanges();

        _repo.Remove(r);
        _context.SaveChanges();

        var removed = _context.Reviews.Find(r.Id);
        Assert.That(removed!.IsRemoved, Is.True);
    }

    [Test]
    public void Query_ShouldReturnQueryable_AndAllowFiltering()
    {
        var u = new User { Email = "eve@example.com", PasswordHash = "x" };
        var b = new Book { Name = "Hyperion", Description = "Shrike." };
        _context.AddRange(u, b);
        _context.SaveChanges();

        _context.Reviews.AddRange(
            new Review { BookId = b.Id, UserId = u.Id, Rating = (Rating)5, Comment = "wow" },
            new Review { BookId = b.Id, UserId = u.Id, Rating = (Rating)3, Comment = "avg" }
        );
        _context.SaveChanges();

        var high = _repo.Query().Where(r => r.Rating >= (Rating)4);
        Assert.That(high.Count(), Is.EqualTo(1));
        Assert.That(high.Single().Comment, Is.EqualTo("wow"));
    }
    #endregion

    #region ReviewRepository Specific
    [Test]
    public async Task GetReviewsByBookIdAsync_ShouldReturnOnlyGivenBookReviews()
    {
        var u1 = new User { Email = "nick@example.com", PasswordHash = "x" };
        var u2 = new User { Email = "mia@example.com", PasswordHash = "x" };
        var b1 = new Book { Name = "Book A", Description = "A desc" };
        var b2 = new Book { Name = "Book B", Description = "B desc" };
        _context.AddRange(u1, u2, b1, b2);
        await _context.SaveChangesAsync();

        _context.Reviews.AddRange(
            new Review { BookId = b1.Id, UserId = u1.Id, Rating = (Rating)4, Comment = "for A #1" },
            new Review { BookId = b1.Id, UserId = u2.Id, Rating = (Rating)5, Comment = "for A #2" },
            new Review { BookId = b2.Id, UserId = u1.Id, Rating = (Rating)2, Comment = "for B #1" }
        );
        await _context.SaveChangesAsync();

        var onlyA = await _repo.GetReviewsByBookIdAsync(b1.Id);

        Assert.That(onlyA.Count, Is.EqualTo(2));
        Assert.That(onlyA.All(r => r.BookId == b1.Id), Is.True);
        Assert.That(onlyA.Select(r => r.Comment), Does.Contain("for A #1").And.Contain("for A #2"));
    }
    #endregion
}
