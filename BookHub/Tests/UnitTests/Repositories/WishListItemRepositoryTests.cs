using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.WishListItem;
using Microsoft.EntityFrameworkCore;

namespace Tests.UnitTests.Repositories;

[TestFixture]
public class WishListItemRepositoryTests
{
    private BookHubDbContext _dbContext = null!;
    private WishListItemRepository _wishListItemRepository = null!;

    [SetUp]
    public void SetUp()
    {
        var opt = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _dbContext = new BookHubDbContext(opt);
        _wishListItemRepository = new WishListItemRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    #region BaseRepositoryTests
    
    [Test]
    public async Task Add_And_GetByIdAsync_ShouldWork()
    {
        var wishListItem = new WishlistItem { };
        _wishListItemRepository.Add(wishListItem);
        await _dbContext.SaveChangesAsync();
        
        var foundWishList = await _wishListItemRepository.GetByIdAsync(1);
        Assert.That(foundWishList, Is.Not.Null);
        Assert.That(foundWishList.Id, Is.EqualTo(1));
    }
    
    [Test]
    public void RemoveAsync_ShouldSetIsRemoved()
    {
        var wishListItem = new WishlistItem {  };
        _dbContext.WishlistItems.AddAsync(wishListItem);
        _dbContext.SaveChanges();
        
        _wishListItemRepository.Remove(wishListItem);
        _dbContext.SaveChanges();
        
        var removedWishListItem = _dbContext.WishlistItems.Find(wishListItem.Id);
        Assert.That(removedWishListItem!.IsRemoved, Is.True);
    }
    #endregion
    
    #region WishListRepositoryTests

    [Test]
    public async Task GetWishListItemByWishListIdAndBookId_ShouldReturnWishListItem()
    {
        var book = new Book {Name = "Dune", Description = "A science fiction novel by Frank Herbert."};
        var wishList = new WishList { Name = "Wish List"};
        var wishListItem = new WishlistItem { Wishlist = wishList, Book = book };
        
        _dbContext.Add(wishListItem);
        await _dbContext.SaveChangesAsync();

        var foundWishListItem = _wishListItemRepository.GetWishListItemByWishListIdAndBookIdAsync(1, 1);
        
        Assert.That(foundWishListItem, Is.Not.Null);
        Assert.That(foundWishListItem.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetWishListItemByWishListId_ShouldReturnWishListItems()
    {
        var book1 = new Book {Name = "Dune", Description = "A science fiction novel by Frank Herbert."};
        var book2 = new Book {Name = "Dune 2", Description = "A science fiction novel by Frank Herbert."};
        var wishList = new WishList { Name = "Wish List"};
        
        _dbContext.WishlistItems.AddRange(
            new WishlistItem { Wishlist = wishList, Book = book1 },
            new WishlistItem { Wishlist = wishList, Book = book2 }
        );
        await _dbContext.SaveChangesAsync();
        
        var wishListItems = await _wishListItemRepository.GetWishListItemsByWishListIdAsync(1);
        
        Assert.That(wishListItems, Is.Not.Null);
        Assert.That(wishListItems, Has.Count.EqualTo(2));
        Assert.That(wishListItems[0].BookId, Is.EqualTo(1));
        Assert.That(wishListItems[1].BookId, Is.EqualTo(2));
    }
    #endregion
}