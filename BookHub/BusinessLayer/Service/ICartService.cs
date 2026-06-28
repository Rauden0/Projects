using BusinessLayer.Dto.Cart;
using DataAccessLayer.Models;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;

namespace BusinessLayer.Service;

public interface ICartService
{
    Task<Result<CartDto>> GetCart(int id);
    Task<Result<CartDto>> GetCartByUserId(int userId);
    Task<Result<List<CartDto>>> GetCarts(ODataQueryOptions<CartDto>? odataQueryOptions);
    Task<Result<CartDto>> AddCart(CreateCartDto createCartDto);
    Task DeleteCart(int id);
    Task<Result<CartDto>> AddCartItemToCart(CartItemDto cartItemDto);
    Task RemoveCartItemFromCart(CartItemDto cartItemDto);
    Task RemoveCartItemFromCartUnsaved(CartItemDto cartItemDto);
    Task<Result<List<CartItemDto>>> GetCartItemsByCartIdAsync(int cartId);
    
    Task<Result<CartDto>> ApplyCoupon(int cartId, string code);
    
    Task<Result<CartDto>> RemoveCoupon(int cartId);
}