using BusinessLayer.Dto.Wishlist;
using BusinessLayer.Service;
using DataAccessLayer;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.WishList;
using DataAccessLayer.Repository.WishListItem;
using Moq;

namespace Tests.UnitTests.Servicies;

[TestFixture]
public class WishListServiceTests
{
    private Mock<IUnitOfWork> _uowMock = null!;
    private Mock<IWishListRepository> _wishListRepositoryMock = null!;
    private Mock<IWishListItemRepository> _wishListItemRepositoryMock = null!;
    private WishListService _wishListService = null!;

    [SetUp]
    public void SetUp()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _wishListRepositoryMock = new Mock<IWishListRepository>();
        _wishListItemRepositoryMock = new Mock<IWishListItemRepository>();
        _uowMock.Setup(u => u.WishLists).Returns(_wishListRepositoryMock.Object);
        _uowMock.Setup(u => u.WishlistItems).Returns(_wishListItemRepositoryMock.Object);
        _wishListService = new WishListService(_uowMock.Object);
    }

    #region GetWishList
    
    [Test]
    public async Task? GetWishList_ShouldReturnResult()
    {
        var wishList = new WishList { Name = "Best books"};
        _wishListRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(wishList);

        var result = await _wishListService.GetWishList(1);
        
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Match(m => m.Name, e => e.Message), Is.EqualTo("Best books"));
    }

    [Test]
    public async Task GetWishList_ShouldReturnError()
    {
        _wishListRepositoryMock.Setup(r => r.GetByIdAsync(30)).ReturnsAsync((WishList?)null);
        var result = await _wishListService.GetWishList(30);
        
        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("Wishlist 30 not found"));
    }
    #endregion
    
    #region AddWishList

    [Test]
    public async Task AddWishLists_ShouldReturnResult()
    {
        var requestWithList = new CreateWithListDto {Name = "Best books", UserId = 1};
        
        var result = await _wishListService.AddWishList(requestWithList);
        
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Match(m => m.Name, e => e.Message), Is.EqualTo("Best books"));
        _wishListRepositoryMock.Verify(r => r.Add(It.IsAny<WishList>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
    #endregion

    #region UpdateWishList

    [Test]
    public async Task UpdateWishList_ShouldReturnResult()
    {
        var wishList = new WishList { Id = 1, Name = "Books" };
        var requestWithList = new UpdateWishListDto {Name = "Best books"};
        _wishListRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(wishList);

        var result = await _wishListService.UpdateWishList(1, requestWithList.Name);
        
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(wishList.Name, Is.EqualTo("Best books"));
        _wishListRepositoryMock.Verify(r => r.Update(wishList), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task UpdateWishList_ShouldReturnError_NoWishList()
    {
        var requestWithList = new UpdateWishListDto {Name = "Best books"};
        _wishListRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((WishList?)null);
        
        var result = await _wishListService.UpdateWishList(2, requestWithList.Name);
        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("Wishlist 2 not found"));
    }
    #endregion
    
    #region DeleteWishList

    [Test]
    public async Task DeleteWishList_ShouldRemoveWishList()
    {
        var wishList = new WishList { Id = 1, Name = "Best books" };
        _wishListRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(wishList);
        
        _uowMock.Setup(u => u.ExecuteInTransactionAsync(
                It.IsAny<Func<Task>>(),
                null
            ))
            .Returns<Func<Task>, Func<Task<bool>>?>(async (operation, verify) => await operation());

        await _wishListService.DeleteWishList(1);
        
        _wishListRepositoryMock.Verify(r => r.Remove(wishList), Times.Once);
        _uowMock.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), null), Times.Once);
    }

    [Test]
    public async Task DeleteWishList_ShouldReturnError_NoWishList()
    {
        _uowMock.Setup(u => u.ExecuteInTransactionAsync(
                It.IsAny<Func<Task>>(), 
                null
            ))
            .Returns<Func<Task>, Func<Task<bool>>?>(async (operation, _) => await operation());
        
        await _wishListService.DeleteWishList(1);
        _wishListRepositoryMock.Verify(r => r.Remove(It.IsAny<WishList>()), Times.Never);
    }
    #endregion

    #region WishListTests

    [Test]
    public async Task AddWishListItemToWishList_ShouldReturnOk()
    {
        var wishList = new WishList {Id = 1, Name = "Best books"};
        _wishListRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(wishList);

        var result = await _wishListService.AddBookToWishList(1, 1);
        
        Assert.That(result.IsSuccess, Is.True);
        _wishListItemRepositoryMock.Verify(r => r.Add(It.IsAny<WishlistItem>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task AddWishListItemToWishList_ShouldReturnError_NoWishList()
    {
        _wishListRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((WishList?) null);

        var result = await _wishListService.AddBookToWishList(1, 1);
        
        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("Wishlist 1 not found"));
    }

    [Test]
    public async Task RemoveWishListItemFromWishList_ShouldReturnOk()
    {
        var wishListItem = new WishlistItem { };
        _wishListItemRepositoryMock.Setup(r => r.GetWishListItemByWishListIdAndBookIdAsync(1, 1)).ReturnsAsync(wishListItem);

        _uowMock.Setup(u => u.ExecuteInTransactionAsync(
                It.IsAny<Func<Task>>(), 
                null
            ))
            .Returns<Func<Task>, Func<Task<bool>>?>(async (operation, _) => await operation());
        
        await _wishListService.RemoveBookFromWishList(1, 1);
        
        _wishListItemRepositoryMock.Verify(r => r.GetWishListItemByWishListIdAndBookIdAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task GetWishlistItemsInWishList_ShouldReturnResult()
    {
        var wishList = new WishList {Id = 1, Name = "Best books"};
        var wishListItemList = new List<WishlistItem>();
        wishListItemList.Add(new WishlistItem { WishlistId = 1});
        _wishListRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(wishList);
        _wishListItemRepositoryMock.Setup(r => r.GetWishListItemsByWishListIdAsync(1)).ReturnsAsync(wishListItemList);

        var result = await _wishListService.GetWishlistItemsInWishList(1);
        
        Assert.That(result.IsSuccess, Is.True);
    }
    
    [Test]
    public async Task GetWishlistItemsInWishList_ShouldReturnError_NoWishlist()
    {
        _wishListRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((WishList?)null);

        var result = await _wishListService.GetWishlistItemsInWishList(1);
        
        Assert.That(result.IsFaulted, Is.True);
    }
    #endregion
}