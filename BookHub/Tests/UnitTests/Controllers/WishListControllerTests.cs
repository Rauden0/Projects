using BookHub.Controller;
using BusinessLayer.Dto.Wishlist;
using BusinessLayer.Service;
using DataAccessLayer.Models;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Moq;

namespace Tests.UnitTests.Controllers;

[TestFixture]
public class WishListControllerTests
{
    private Mock<IWishListService> _wishListServiceMock = null!;
    private WishListController _wishListController = null!;
    
    [SetUp]    
    public void Setup()
    {
        _wishListServiceMock = new Mock<IWishListService>();
        _wishListController = new WishListController(_wishListServiceMock.Object);
    }

    #region GetWishList
    
    [Test]
    public async Task GetGenre_ShouldReturnOk()
    {
        var wishListDto = new WishListDto() { Name = "Best books"};
        _wishListServiceMock.Setup(s => s.GetWishList(1)).ReturnsAsync(new Result<WishListDto>(wishListDto));

        var result = await _wishListController.GetWishList(1);
        
        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok.Value, Is.EqualTo(wishListDto));
    }
    
    [Test]
    public async Task GetGenre_ShouldReturnNotFound()
    {
        _wishListServiceMock.Setup(s => s.GetWishList(1))
            .ReturnsAsync(new Result<WishListDto>(new Exception("Wishlist 1 not found")));

        var result = await _wishListController.GetWishList(1);
        
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound!.Value, Is.EqualTo("Wishlist 1 not found"));
    }
    #endregion
    
    #region GetWishLists

    [Test]
    public async Task GetWishLists_ShouldReturnOk()
    {
        var wishListList = new List<WishListDto> { new WishListDto { Name = "Best books" } };
        _wishListServiceMock.Setup(s => s.GetWishLists(It.IsAny<ODataQueryOptions<WishListDto>>()))
            .ReturnsAsync(wishListList);

        var result = await _wishListController.GetWishLists(null);
        
        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok.Value, Is.EqualTo(wishListList));
    }

    [Test]
    public async Task GetWishLists_ShouldReturnNotFound()
    {
        _wishListServiceMock.Setup(s => s.GetWishLists(It.IsAny<ODataQueryOptions<WishListDto>>()))
            .ReturnsAsync(new Result<List<WishListDto>>(new Exception("Error fetching wishlists")));
        
        var result = await _wishListController.GetWishLists(null);
        
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound!.Value, Is.EqualTo("Error fetching wishlists"));
    }
    #endregion

    #region  AddWishList

    [Test]
    public async Task AddWishList_ShouldReturnOk()
    {
        var wishListRequest = new CreateWithListDto { Name = "Best books" };
        var wishListDto = new WishListDto { Name = "Best books" };
        _wishListServiceMock.Setup(s => s.AddWishList(wishListRequest)).ReturnsAsync(new Result<WishListDto>(wishListDto));
        
        var result = await _wishListController.AddWishList(wishListRequest);
        var ok = result as OkObjectResult;
        
        Assert.That(ok, Is.Not.Null);
    }
    #endregion
    
    #region  UpdateWishList
    
    [Test]
    public async Task UpdateWishList_ShouldReturnOk()
    {
        var wishListRequest = new UpdateWishListDto { Name = "Best books" };
        var wishListDto = new WishListDto() { Name = "Books" };
        _wishListServiceMock.Setup(s => s.UpdateWishList(1, wishListRequest.Name))
            .ReturnsAsync(new Result<WishListDto>(wishListDto));
        
        var result = await _wishListController.UpdateWishList(1, wishListRequest);
        
        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(wishListDto));
    }

    [Test]
    public async Task UpdateWishList_ShouldReturnNotFound_NoWishlist()
    {
        var wishListRequest = new UpdateWishListDto { Name = "Best books" };
        _wishListServiceMock.Setup(s => s.UpdateWishList(1, wishListRequest.Name))
            .ReturnsAsync(new Result<WishListDto>(new Exception("Wishlist 1 not found")));
        
        var result = await _wishListController.UpdateWishList(1, wishListRequest);
        
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }
    #endregion
    
    #region  DeleteWishList

    [Test]
    public async Task DeleteWishList_ShouldReturnOk()
    {
        _wishListServiceMock.Setup(s => s.DeleteWishList(1)).Returns(Task.CompletedTask);
        
        var result = await _wishListController.DeleteWishList(1);
        
        Assert.That(result, Is.TypeOf<NoContentResult>());
    }
    #endregion

    #region WishListItemTest

    [Test]
    public async Task AddWishListItemTest_ShouldReturnOk()
    {
        var addWishListItemDto = new AddWishListItemDto { WishlistId = 1, BookId = 1 };
        var wishListDto = new WishListDto { Name = "Best Books" };
        _wishListServiceMock.Setup(s => s.AddBookToWishList(1, 1)).ReturnsAsync(new Result<WishListDto>(wishListDto));

        var result = await _wishListController.AddBookToWishList(addWishListItemDto);
        var ok = result as OkObjectResult;
        
        Assert.That(ok, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task RemoveBookFromWishList_ShouldReturnNoContent()
    {
        var removeWishListItemDto = new RemoveWishListItemDto { WishlistId = 1, BookId = 1 };
        _wishListServiceMock.Setup(s => s.RemoveBookFromWishList(1, 1)).Returns(Task.CompletedTask);
        
        var result = await _wishListController.RemoveBookFromWishList(removeWishListItemDto);
        
        Assert.That(result, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task GetWishlistItemsInWishList_ShouldReturnResult()
    {
        var wishListItemList = new List<WishListItemDto>();
        wishListItemList.Add(new WishListItemDto { BookId = 1 });
        _wishListServiceMock.Setup(s => s.GetWishlistItemsInWishList(1)).ReturnsAsync(new Result<List<WishListItemDto>>(wishListItemList));

        var result = await _wishListController.GetGetWishlistItemsInWishListInWishList(1);
        var ok = result as OkObjectResult;
        
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(wishListItemList));
    }
    #endregion
}