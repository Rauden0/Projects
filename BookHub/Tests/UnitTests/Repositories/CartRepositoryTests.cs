using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Cart;
using Microsoft.EntityFrameworkCore;

namespace Tests.UnitTests.Repositories;

[TestFixture]
public class CartRepositoryTests
{
    private BookHubDbContext _context = null!;
    private CartRepository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BookHubDbContext(options);
        _repo = new CartRepository(_context);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task GetCartByUserIdAsync_ShouldReturnCartWithItems()
    {
        var userId = 123;
        var book = new Book
        {
            Id = 1,
            Name = "Test Book",
            StockQuantity = 10, 
            Price = 20,
            Description = "adasd"
        };
        _context.Books.Add(book);
        var cart = new Cart 
        { 
            UserId = userId, 
            CartItems = new List<CartItem> { new() { BookId = 1, Quantity = 1 } } 
        };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        var result = await _repo.GetCartByUserIdAsync(userId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserId, Is.EqualTo(userId));
        Assert.That(result.CartItems, Is.Not.Empty);
        Assert.That(_context.Entry(result).State, Is.EqualTo(EntityState.Detached));
    }

    [Test]
    public async Task GetCartByUserIdAsync_ShouldReturnNull_WhenUserHasNoCart()
    {
        var result = await _repo.GetCartByUserIdAsync(999);
        Assert.That(result, Is.Null);
    }
}