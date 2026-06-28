using BusinessLayer.Dto.Cart;
using BusinessLayer.Extension;
using BusinessLayer.Mapping;
using DataAccessLayer;
using DataAccessLayer.Models;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service;

public class CartService : ICartService
{
    private readonly IUnitOfWork _uow;

    public CartService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<CartDto>> GetCart(int id)
    {
        var cart = await _uow.Carts.GetByIdAsync(id);
        return cart is not null
            ? CartMapper.ToDto(cart)
            : new Result<CartDto>(new Exception($"Cart {id} not found"));
    }
    
    public async Task<Result<CartDto>> GetCartByUserId(int userId)
    {
        var cart = await _uow.Carts.GetCartByUserIdAsync(userId);
        return cart is not null
            ? CartMapper.ToDto(cart)
            : new Result<CartDto>(new Exception($"Cart owned by user:{userId} not found"));
    }

    public async Task<Result<List<CartDto>>> GetCarts(ODataQueryOptions<CartDto>? options)
    {
        return await CartMapper.ProjectToDto(_uow.Carts.Query()).ApplyIfNotNull(options).ToListAsync();
    }

    public async Task<Result<CartDto>> AddCart(CreateCartDto createCartDto)
    {
        var cart = new Cart
        {
            UserId = createCartDto.UserId
        };
        
        _uow.Carts.Add(cart);
        await _uow.SaveChangesAsync();
        return CartMapper.ToDto(cart);
    }

    public async Task DeleteCart(int id)
    {
        await _uow.ExecuteInTransactionAsync(async () =>
        {
            var cart = await _uow.Carts.GetByIdAsync(id); 
            if (cart is not null) 
                _uow.Carts.Remove(cart); 
            await _uow.SaveChangesAsync();
        });
    }

    public async Task<Result<CartDto>> AddCartItemToCart(CartItemDto cartItemDto)
    {
        var cart = await _uow.Carts.GetByIdAsync(cartItemDto.CartId);
        if (cart is null)
            return new Result<CartDto>(new Exception($"Cart {cartItemDto.CartId} not found"));
        
        var book = await _uow.Books.GetByIdAsync(cartItemDto.BookId);
        if (book is null)
            return new Result<CartDto>(new Exception($"Book {cartItemDto.BookId} not found"));
        
        var existingCartItem = await _uow.CartItems.GetCartItemByCartIdAndBookIdAsync(cartItemDto.CartId, cartItemDto.BookId);
        if (existingCartItem is null)
        {
            var cartItem = new CartItem
            {
                CartId = cartItemDto.CartId,
                BookId = cartItemDto.BookId,
                Quantity = cartItemDto.Quantity
            };
            _uow.CartItems.Add(cartItem);
        }
        else
        {
            existingCartItem.Quantity += cartItemDto.Quantity;
            _uow.CartItems.Update(existingCartItem);
        }
        
        await _uow.SaveChangesAsync();
        return CartMapper.ToDto(cart);
    }

    public async Task RemoveCartItemFromCart(CartItemDto cartItemDto)
    {
        await _uow.ExecuteInTransactionAsync(async () =>
        {
            var cartItem = await _uow.CartItems.GetCartItemByCartIdAndBookIdAsync(cartItemDto.CartId, cartItemDto.BookId);
            if (cartItem is not null)
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity -= 1;
                    _uow.CartItems.Update(cartItem);
                }
                else
                {
                    _uow.CartItems.Remove(cartItem);
                }
            await _uow.SaveChangesAsync();
        });
    }

    public async Task RemoveCartItemFromCartUnsaved(CartItemDto cartItemDto)
    {
        var cartItem = await _uow.CartItems.GetCartItemByCartIdAndBookIdAsync(cartItemDto.CartId, cartItemDto.BookId)
            ?? throw new Exception($"CartItem with Card {cartItemDto.CartId} and Book {cartItemDto.BookId} not found");
        _uow.CartItems.Remove(cartItem);
    }

    public async Task<Result<List<CartItemDto>>> GetCartItemsByCartIdAsync(int cartId)
    {
        var cart = await _uow.Carts.GetByIdAsync(cartId);
        if (cart is null)
            return new Result<List<CartItemDto>>(new Exception($"Cart {cartId} not found"));
        var cartItems = await _uow.CartItems.GetCartItemsByCartIdAsync(cartId);
        return CartItemMapper.ProjectToDto(cartItems.AsQueryable()).ToList();
    }
    
    public async Task<Result<CartDto>> ApplyCoupon(int cartId, string code)
    {
        var cart = await _uow.Carts.GetByIdAsync(cartId);
        if (cart is null) return new Result<CartDto>(new Exception("Cart not found"));

        var coupon = await _uow.Coupons.Query()
            .Include(c => c.GiftCard)
            .FirstOrDefaultAsync(c => c.Code == code);

        if (coupon is null) return new Result<CartDto>(new Exception("Code not found"));

        if (coupon.IsUsed) return new Result<CartDto>(new Exception("Code was already used"));
    
        var now = DateTime.UtcNow;
        if (now < coupon.GiftCard.ValidFrom || now > coupon.GiftCard.ValidTo)
            return new Result<CartDto>(new Exception("Code is not valid at this time"));

        cart.AppliedCouponId = coupon.Id;
        _uow.Carts.Update(cart);
        await _uow.SaveChangesAsync();

        return CartMapper.ToDto(cart);
    }

    public async Task<Result<CartDto>> RemoveCoupon(int cartId)
    {
        var cart = await _uow.Carts.GetByIdAsync(cartId);
        if (cart is null) 
            return new Result<CartDto>(new Exception("Cart not found"));

        cart.AppliedCouponId = null;
    
        _uow.Carts.Update(cart);
        await _uow.SaveChangesAsync();

        return new Result<CartDto>(CartMapper.ToDto(cart));
    }
}