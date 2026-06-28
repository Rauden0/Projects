using BookHub.Mvc.Models.User.WishList;
using BusinessLayer.Dto.Book;
using BusinessLayer.Dto.User;
using BusinessLayer.Dto.Wishlist;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookHub.Mvc.Controller;

public class UserWishListController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly IBookService _bookService;
    private readonly IWishListService _wishListService;
    private readonly IUserService _userService;

    public UserWishListController(IBookService bookService, IWishListService wishListService, IUserService userService)
    {
        _bookService = bookService;
        _wishListService = wishListService;
        _userService = userService;
    }

    private async Task FillModel(UserWishListViewModel vm)
    {
        var email = User.Identity?.Name ?? "unknown";
        
        var userResult = await _userService.GetUserByEmailAsync(email!);
        var userDto = userResult.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading user {email}: {err.Message}");
            return new UserDto();
        });
        
        var wishListResult = await _wishListService.GetWishListsByUserId(userDto.Id);
        var wishListsDto = wishListResult.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading wishList: {err.Message}");
            return new List<WishListDto>();
        });

        vm.WishLists = wishListsDto.Select(wl => new SelectListItem
        {
            Value = wl.Id.ToString(),
            Text = wl.Name
        });

        vm.WishListDetailViewModels = wishListsDto.Select(wldvm => new UserWishListDetailViewModel
        {
            Id = wldvm.Id,
            Name = wldvm.Name
        }).ToList();
    }

    private async Task FillDetailModel(int id, UserWishListDetailViewModel dvm)
    {
        var wishListResult = await _wishListService.GetWishList(id);
        var wishListDto = wishListResult.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading wishList: {err.Message}");
            return new WishListDto();
        });
        
        var wishListItemListResult = await _wishListService.GetWishlistItemsInWishList(id);
        var wishListItemList = wishListItemListResult.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading cart item list: {err.Message}");
            return new List<WishListItemDto>();
        });
        
        var bookList = new List<BookDto>();
        foreach (var wishListItem in wishListItemList)
        {
            var bookResult = await _bookService.GetBook(wishListItem.BookId);
            var bookDto = bookResult.Match(b => b, err =>
            {
                ModelState.AddModelError(string.Empty, $"Error loading book {wishListItem.BookId}: {err.Message}");
                return new BookDto();
            });
            bookList.Add(bookDto);
        }

        dvm.Id = wishListDto.Id;
        dvm.Name = wishListDto.Name;
        dvm.WishListItems = wishListItemList;
        dvm.Books = bookList;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new UserWishListViewModel();
        await FillModel(vm);
        if (!ModelState.IsValid)
        {
            ViewData["Error"] = "Error loading wish lists";
        }
        return View(vm);
    }
    
    public async Task<IActionResult> Details(int id)
    {
        var dvm = new UserWishListDetailViewModel();
        await FillDetailModel(id, dvm);
        
        if (!ModelState.IsValid)
        {
            ViewData["Error"] = "Error loading model";
        }    
        return View(dvm);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int bookId, int wishListId)
    {
        await _wishListService.RemoveBookFromWishList(wishListId, bookId);
        
        var dvm = new UserWishListDetailViewModel();
        await FillDetailModel(wishListId, dvm);
        
        if (!ModelState.IsValid)
        {
            ViewData["Error"] = "Error loading model";
        }    
        return RedirectToAction(nameof(Details), new { id = wishListId });
    }
    
    public async Task<IActionResult> DeleteWishList(int id)
    {
        await _wishListService.DeleteWishList(id);
        
        var vm = new UserWishListViewModel();
        await FillModel(vm);
        
        if (!ModelState.IsValid)
        {
            ViewData["Error"] = "Error loading model";
        }    
        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> Create()
    {
        var dvm = new UserWishListDetailViewModel();
        return View(dvm);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserWishListDetailViewModel dvm)
    {
        if (!ModelState.IsValid)
        {
            return View(dvm);
        }

        var email = User.Identity?.Name ?? "unknown";
        
        var userResult = await _userService.GetUserByEmailAsync(email!);
        var userDto = userResult.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading user {email}: {err.Message}");
            return new UserDto();
        });
        
        if (!ModelState.IsValid)
        {
            ViewData["Error"] = "Unknown user";
            return View(dvm);
        }

        var request = new CreateWithListDto
        {
            UserId = userDto.Id,
            Name = dvm.Name,
        };

        var result = await _wishListService.AddWishList(request);

        var error = result.Match<string?>(
            _ => null,
            ex => ex.Message);

        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(dvm);
        }

        return RedirectToAction(nameof(Index));
    }
}