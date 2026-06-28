using BusinessLayer.Dto.Wishlist;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace BookHub.Controller;

[ApiController]
[Route("/wishlists")]
public class WishListController : ControllerBase
{
    private readonly IWishListService _wishListService;

    public WishListController(IWishListService wishListService)
    {
        _wishListService = wishListService;
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetWishList(int id)
    {
        var wishList = await _wishListService.GetWishList(id);
        return wishList.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<WishListDto>))]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetWishLists([FromQuery] ODataQueryOptions<WishListDto>? options)
    {
        var wishList = await _wishListService.GetWishLists(options);
        return wishList.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WishListDto))]
    public async Task<IActionResult> AddWishList([FromBody] CreateWithListDto createWithListDto)
    {
        var wishList = await _wishListService.AddWishList(createWithListDto);
        return Ok(wishList);
    }

    [HttpPut]
    [Route("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WishListDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWishList(int id, [FromBody] UpdateWishListDto dto)
    {
        var wishList = await _wishListService.UpdateWishList(id, dto.Name);
        return wishList.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }

    [HttpDelete]
    [Route("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteWishList(int id)
    {
        await _wishListService.DeleteWishList(id);
        return NoContent();
    }

    [HttpPost]
    [Route("items")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WishListDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddBookToWishList([FromBody] AddWishListItemDto dto)
    {
        var wishList = await _wishListService.AddBookToWishList(dto.WishlistId, dto.BookId);
        return wishList.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }

    [HttpDelete]
    [Route("items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveBookFromWishList([FromBody] RemoveWishListItemDto dto)
    {
        await _wishListService.RemoveBookFromWishList(dto.WishlistId, dto.BookId);
        return NoContent();
    }

    [HttpGet]
    [Route("{id}/items")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<WishListItemDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGetWishlistItemsInWishListInWishList(int id)
    {
        var items = await _wishListService.GetWishlistItemsInWishList(id);
        return items.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }
}