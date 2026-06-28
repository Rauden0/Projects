using DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using BusinessLayer.Service;
using BusinessLayer.Dto.Review;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Review;
using DataAccessLayer;

namespace Tests.UnitTests.Servicies;

[TestFixture]
public class ReviewServiceTests
{
    private BookHubDbContext _ctx = null!;
    private ReviewRepository _reviewRepo = null!;
    private Mock<IUnitOfWork> _uow = null!;
    private ReviewService _service = null!;

    [SetUp]
    public void Setup()
    {
        var opts = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase(databaseName: $"reviews_{Guid.NewGuid()}")
            .Options;

        _ctx = new BookHubDbContext(opts);
        _reviewRepo = new ReviewRepository(_ctx);

        _uow = new Mock<IUnitOfWork>();
        _uow.Setup(u => u.Reviews).Returns(_reviewRepo);
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

        _service = new ReviewService(_uow.Object);
    }

    [TearDown]
    public void TearDown() => _ctx.Dispose();
    [Test]
    public async Task GetReview_ReturnsDto_WhenExists()
    {
        var user = new User { Email = "a@a.com", PasswordHash = "x" };
        var book = new Book { Name = "Dune", Description = "Sci-fi epic." };
        _ctx.Users.Add(user);
        _ctx.Books.Add(book);
        await _ctx.SaveChangesAsync();

        var review = new Review { BookId = book.Id, UserId = user.Id, Rating = (Rating)5, Comment = "Great" };
        _ctx.Reviews.Add(review);
        await _ctx.SaveChangesAsync();

        var res = await _service.GetReview(review.Id);

        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Match(d => d.Comment, _ => ""), Is.EqualTo("Great"));
        Assert.That(res.Match(d => d.Rating, _ => (Rating)0), Is.EqualTo((Rating)5));
    }

    [Test]
    public async Task GetReview_ReturnsError_WhenMissing()
    {
        var res = await _service.GetReview(999);
        Assert.That(res.IsFaulted, Is.True);
        Assert.That(res.ToString(), Does.Contain("Review 999 not found"));
    }
    
    [Test]
    public async Task GetReviews_ReturnsMappedList_WhenOptionsNull()
    {
        var u = new User { Email = "u@x.com", PasswordHash = "x" };
        var b = new Book { Name = "Foundation", Description = "Classic." };
        _ctx.AddRange(u, b);
        await _ctx.SaveChangesAsync();

        _ctx.Reviews.AddRange(
            new Review { BookId = b.Id, UserId = u.Id, Rating = (Rating)4, Comment = "Nice" },
            new Review { BookId = b.Id, UserId = u.Id, Rating = (Rating)2, Comment = "Meh" }
        );
        await _ctx.SaveChangesAsync();

        var res = await _service.GetReviews(options: null);

        Assert.That(res.IsSuccess, Is.True);
        var list = res.Match(d => d, _ => new List<ReviewDto>());
        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.Select(x => x.Comment), Does.Contain("Nice").And.Contain("Meh"));
    }
    
    [Test]
    public async Task AddReview_PersistsEntity_AndReturnsDto()
    {
        var u = new User { Email = "new@user.com", PasswordHash = "x" };
        var b = new Book { Name = "Neuromancer", Description = "Cyberpunk." };
        _ctx.AddRange(u, b);
        await _ctx.SaveChangesAsync();

        var req = new CreateReviewDto { BookId = b.Id, UserId = u.Id, Rating = (Rating)5, Comment = "Excellent" };

        var res = await _service.AddReview(req);

        Assert.That(res.IsSuccess, Is.True);
        var dto = res.Match(d => d, _ => null!);
        Assert.That(dto.Comment, Is.EqualTo("Excellent"));
        Assert.That(dto.Rating, Is.EqualTo((Rating)5));

        // ověř, že se fyzicky uložilo
        var stored = await _ctx.Reviews.FirstOrDefaultAsync(r => r.Comment == "Excellent");
        Assert.That(stored, Is.Not.Null);

        _uow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateReview_ChangesFields_AndSaves()
    {
        var u = new User { Email = "upd@user.com", PasswordHash = "x" };
        var b = new Book { Name = "Book X", Description = "Desc" };
        _ctx.AddRange(u, b);
        await _ctx.SaveChangesAsync();

        var r = new Review { BookId = b.Id, UserId = u.Id, Rating = (Rating)2, Comment = "meh" };
        _ctx.Reviews.Add(r);
        await _ctx.SaveChangesAsync();

        var req = new UpdateReviewDto { Rating = (Rating)4, Comment = "better" };
        var res = await _service.UpdateReview(r.Id, req);

        Assert.That(res.IsSuccess, Is.True);
        var dto = res.Match(d => d, _ => null!);
        Assert.That(dto.Rating, Is.EqualTo((Rating)4));
        Assert.That(dto.Comment, Is.EqualTo("better"));

        var reloaded = await _ctx.Reviews.FindAsync(r.Id);
        Assert.That(reloaded!.Rating, Is.EqualTo((Rating)4));
        Assert.That(reloaded.Comment, Is.EqualTo("better"));

        _uow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateReview_ReturnsError_WhenNotFound()
    {
        var req = new UpdateReviewDto { Rating = (Rating)3, Comment = "x" };
        var res = await _service.UpdateReview(12345, req);

        Assert.That(res.IsFaulted, Is.True);
        Assert.That(res.ToString(), Does.Contain("Review 12345 not found"));
    }

    [Test]
    public async Task DeleteReview_Removes_WhenExists()
    {
        var u = new User { Email = "del@user.com", PasswordHash = "x" };
        var b = new Book { Name = "Book Y", Description = "Desc" };
        _ctx.AddRange(u, b);
        await _ctx.SaveChangesAsync();

        var r = new Review { BookId = b.Id, UserId = u.Id, Rating = (Rating)3, Comment = "to delete" };
        _ctx.Reviews.Add(r);
        await _ctx.SaveChangesAsync();

        await _service.DeleteReview(r.Id);
        
        var reloaded = await _ctx.Reviews.FindAsync(r.Id);
        Assert.That(reloaded, Is.Not.Null);
        Assert.That(reloaded!.IsRemoved, Is.True);
        var active = _reviewRepo.Query().FirstOrDefault(x => x.Id == r.Id);
        Assert.That(active, Is.Null);

        _uow.Verify(uow => uow.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), null), Times.Once);
        _uow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Test]
    public async Task DeleteReview_DoesNothing_WhenMissing()
    {
        await _service.DeleteReview(999);

        _uow.Verify(uow => uow.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), null), Times.Once);
        _uow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
