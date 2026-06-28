using DataAccessLayer.Data;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.WishList;
using Microsoft.EntityFrameworkCore;

namespace Tests.UnitTests.Repositories;

[TestFixture]
public class WishListRepositoryTests
{
    private BookHubDbContext _dbContext = null!;
    private WishListRepository _wishListRepository = null!;

    [SetUp]
    public void SetUp()
    {
        var opt = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _dbContext = new BookHubDbContext(opt);
        _wishListRepository = new WishListRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }
    
    #region BaseRepositoryTests
    [Test]
    public async Task AddAsync_And_GetByIdAsync_ShouldWork()
    {
        var wishList = new WishList { Name = "Best books"};
        _wishListRepository.Add(wishList);
        await _dbContext.SaveChangesAsync();
        
        var foundWishList = await _wishListRepository.GetByIdAsync(1);
        Assert.That(foundWishList, Is.Not.Null);
        Assert.That(foundWishList.Name, Is.EqualTo("Best books"));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnResult()
    {
        _dbContext.Wishlists.AddRange(
            new WishList { Name = "Best books"},
            new WishList { Name = "Christmas"}
        );
        await _dbContext.SaveChangesAsync();
        
        var wishLists = await _wishListRepository.GetAllAsync();
        Assert.That(wishLists, Is.Not.Empty);
        Assert.That(wishLists, Has.Count.EqualTo(2));
        Assert.That(wishLists.Select(u => u.Name), Does.Contain("Best books").And.Contain("Christmas"));
    }

    [Test]
    public void UpdateAsync_ShouldWork()
    {
        var wishList = new WishList { Name = "Best Books"};
        _dbContext.Wishlists.Add(wishList);
        _dbContext.SaveChanges();
        
        wishList.Name = "Best Books I want to buy";
        _wishListRepository.Update(wishList);
        _dbContext.SaveChanges();
        
        var updatedWishList = _dbContext.Wishlists.Find(wishList.Id);
        Assert.That(updatedWishList!.Name, Is.EqualTo("Best Books I want to buy"));
    }

    [Test]
    public void RemoveAsync_ShouldSetIsRemoved()
    {
        var wishList = new WishList { Name = "Best Books"};
        _dbContext.Wishlists.Add(wishList);
        _dbContext.SaveChanges();
        
        _wishListRepository.Remove(wishList);
        _dbContext.SaveChanges();
        var removedWishList = _dbContext.Wishlists.Find(wishList.Id);
        Assert.That(removedWishList!.IsRemoved, Is.True);
    }

    [Test]
    public void Query_ShouldReturnResult()
    {
        _dbContext.Wishlists.AddRange(
            new WishList { Name = "Best Books"},
            new WishList { Name = "Christmas"}
        );
        _dbContext.SaveChanges();
        
        var wishLists = _wishListRepository.Query().Where(u => u.Name.Contains("Ch"));
        Assert.That(wishLists.Single().Name, Is.EqualTo("Christmas"));
    }
    #endregion
    
    #region WishListRepositoryTests
    
    [Test]
    public async Task GetWishListsByUserIdAsync_ShouldWork()
    {
        var user1 = new User { DisplayName = "Vita", Email = "vita@email.com", PasswordHash = "sda", Role = RoleType.User};
        var user2 = new User { DisplayName = "Petr", Email = "petr@email.com", PasswordHash = "sds", Role = RoleType.User };
        
        _dbContext.Wishlists.AddRange(
            new WishList { Name = "Best Books", User = user1},
            new WishList { Name = "Christmas", User = user2}
        );
        _dbContext.SaveChanges();
        
        var foundWishLists = await _wishListRepository.GetWishListsByUserIdAsync(1);
        
        Assert.That(foundWishLists, Is.Not.Null);
        Assert.That(foundWishLists, Has.Count.EqualTo(1));
        Assert.That(foundWishLists.Single().Name, Is.EqualTo("Best Books"));
    }
    #endregion
}