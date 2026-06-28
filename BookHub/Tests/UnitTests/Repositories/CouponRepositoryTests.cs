namespace Tests.UnitTests.Repositories;

using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Coupon;
using Microsoft.EntityFrameworkCore;

[TestFixture]
public class CouponRepositoryTests
{
    private BookHubDbContext _context = null!;
    private CouponRepository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BookHubDbContext(options);
        _repo = new CouponRepository(_context);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task Query_ShouldFilterCoupons()
    {
        _context.Coupons.AddRange(
            new Coupon { Code = "SAVE10", IsUsed = false },
            new Coupon { Code = "USED50", IsUsed = true }
        );
        await _context.SaveChangesAsync();

        var activeCoupons = _repo.Query().Where(c => !c.IsUsed).ToList();

        Assert.That(activeCoupons.Count, Is.EqualTo(1));
        Assert.That(activeCoupons[0].Code, Is.EqualTo("SAVE10"));
    }
}