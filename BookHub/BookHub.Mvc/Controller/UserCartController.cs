using BookHub.Mvc.Models.User;
using BusinessLayer.Dto.Book;
using BusinessLayer.Dto.Cart;
using BusinessLayer.Dto.User;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Mvc;

namespace BookHub.Mvc.Controller;
public class UserCartController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly IBookService _bookService;
    private readonly ICartService _cartService;
    private readonly IUserService _userService;

    public UserCartController(IBookService bookService, ICartService cartService, IUserService userService)
    {
        _bookService = bookService;
        _cartService = cartService;
        _userService = userService;
    }
    
    private async Task<int> GetUserCartId()
    {
        var email = User.Identity?.Name ?? "unknown";
        
        var userResult = await _userService.GetUserByEmailAsync(email!);
        var userDto = userResult.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading user {email}: {err.Message}");
            return new UserDto();
        });
        
        var cartResult = await _cartService.GetCartByUserId(userDto.Id);
        var cartDto = cartResult.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading cart: {err.Message}");
            return new CartDto();
        });
        
        return cartDto.Id;
    }
        
    private async Task FillModel(UserCartViewModel vm)
    {
        var cartDtoId = await GetUserCartId();
        
        var cartItemsListResult = await _cartService.GetCartItemsByCartIdAsync(cartDtoId);
        var cartItemsList = cartItemsListResult.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading cart item list: {err.Message}");
            return new List<CartItemDto>();
        });
        
        var bookIds = cartItemsList.Select(ci => ci.BookId).Distinct();
        var bookListResult = await _bookService.GetBooksByIdsAsync(bookIds, includeImages: true);
        var bookList = bookListResult.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading books.");
            return new List<BookDto>();
        });
        
        vm.Id = cartDtoId;
        vm.CartItems = cartItemsList; 
        vm.Books = bookList;
    }
    
    public async Task<IActionResult> Index()
    {
        var vm = new UserCartViewModel();
        await FillModel(vm);
        if (!ModelState.IsValid)
        {
            ViewData["Error"] = "Error loading model";
        }
        return View(vm);
    }
    
    public async Task<IActionResult> DecreaseItemQuantity(int id)
    {
        var cartDtoId = await GetUserCartId();
        var cartItemDto = new CartItemDto
        {
            BookId = id,
            CartId = cartDtoId,
            Quantity = 1
        };
        await _cartService.RemoveCartItemFromCart(cartItemDto);
        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> IncreaseItemQuantity(int id)
    {
        var cartDtoId = await GetUserCartId();
        var cartItemDto = new CartItemDto
        {
            BookId = id,
            CartId = cartDtoId,
            Quantity = 1
        };
        var result = await _cartService.AddCartItemToCart(cartItemDto);
        if (result.IsFaulted) 
            ModelState.AddModelError(string.Empty, $"Error adding book {id} to cart.");
        return RedirectToAction(nameof(Index));
    }
}