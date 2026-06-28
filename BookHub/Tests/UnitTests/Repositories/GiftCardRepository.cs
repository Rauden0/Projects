using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.GiftCard;
using Microsoft.EntityFrameworkCore;

namespace Tests.UnitTests.Repositories;

[TestFixture]
public class GiftCardRepositoryTests
{
    private BookHubDbContext _context = null!;
    private GiftCardRepository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BookHubDbContext(options);
        _repo = new GiftCardRepository(_context);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task AddAndGet_ShouldWork()
    {
        var card = new GiftCard { ReductionAmount = 200, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddDays(10) };
        
        _repo.Add(card);
        await _context.SaveChangesAsync();

        var found = await _repo.GetByIdAsync(card.Id);
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.ReductionAmount, Is.EqualTo(200));
    }
}