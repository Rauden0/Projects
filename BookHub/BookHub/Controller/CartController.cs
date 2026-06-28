using BusinessLayer.Dto.Cart;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace BookHub.Controller;

[ApiController]
[Route("/carts")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CartDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCart(int id)
    {
        var cart = await _cartService.GetCart(id);
        return cart.Match<IActionResult>(
            Ok,
                ex => NotFound(ex.Message)
            );
    }
    
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CartDto>))]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetCarts([FromQuery] ODataQueryOptions<CartDto>? options)
    {
        var carts = await _cartService.GetCarts(options);
        return carts.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CartDto))]
    public async Task<IActionResult> AddCart([FromBody] CreateCartDto createCartDto)
    {
        var cart = await _cartService.AddCart(createCartDto);
        return Ok(cart);
    }
    
    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteCart(int id)
    {
        await _cartService.DeleteCart(id);
        return NoContent();
    }
    
    [HttpPost]
    [Route("items")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CartDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddCartItemToCart([FromBody] CartItemDto cartItemDto)
    {
        var cart = await _cartService.AddCartItemToCart(cartItemDto);
        return cart.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }
    
    [HttpDelete]
    [Route("items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveCartItemFromCart([FromBody] CartItemDto cartItemDto)
    {
        await _cartService.RemoveCartItemFromCart(cartItemDto);
        return NoContent();
    }
    
    [HttpPost]
    [Route("{id}/items")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CartItemDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCartItemInCartByCartId(int id)
    {
        var items = await _cartService.GetCartItemsByCartIdAsync(id);
        return items.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }
    
    [HttpPost]
    [Route("{cartId}/apply-coupon")]
    public async Task<IActionResult> ApplyCoupon(int cartId, [FromQuery] string code)
    {
        var result = await _cartService.ApplyCoupon(cartId, code);
        return result.Match<IActionResult>(
            Ok,
            ex => BadRequest(ex.Message)
        );
    }
}