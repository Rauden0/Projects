using BusinessLayer.Dto.Author;
using BusinessLayer.Dto.Book;
using BusinessLayer.Dto.Genre;
using BusinessLayer.Dto.Publisher;
using BusinessLayer.Service;
using BookHub.Mvc.Models.User;
using BookHub.Mvc.Models.User.Review;
using BusinessLayer.Dto.Cart;
using BusinessLayer.Dto.Review;
using BusinessLayer.Dto.User;
using BusinessLayer.Dto.Wishlist;
using DataAccessLayer.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookHub.Mvc.Controller;

public class UserBookController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly IBookService _bookService;
    private readonly IAuthorService _authorService;
    private readonly IGenreService _genreService;
    private readonly IPublisherService _publisherService;
    private readonly ICartService _cartService;
    private readonly IUserService _userService;
    private readonly IReviewService _reviewService;
    private readonly IWishListService _wishListService;

    public UserBookController(
        IBookService bookService,
        IAuthorService authorService,
        IGenreService genreService,
        IPublisherService publisherService,
        ICartService cartService,
        IUserService userService, 
        IReviewService reviewService,
        IWishListService wishListService)
    {
        _bookService = bookService;
        _authorService = authorService;
        _genreService = genreService;
        _publisherService = publisherService;
        _cartService = cartService;
        _userService = userService;
        _reviewService = reviewService;
        _wishListService = wishListService;
    }


    public async Task<IActionResult> AddToCartFromDetail(int id)
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
        
        var cartItemDto = new CartItemDto
        {
            BookId = id,
            CartId = cartDto.Id,
            Quantity = 1
        };
        var result = await _cartService.AddCartItemToCart(cartItemDto);
        if (result.IsFaulted) 
            ModelState.AddModelError(string.Empty, $"Error adding book {id} to cart.");
        
        var bookResult = await _bookService.GetBook(id);
        var book = bookResult.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading book: {err.Message}");
            return new BookDto();
        });
        
        var vm = await BuildDetailVmAsync(book);
        return RedirectToAction(nameof(Detail), new { id = id});
    }
    
    [HttpPost]
    public async Task<IActionResult> AddToWishListFromDetail(int bookId, int wishListId)
    {
        var result = await _wishListService.AddBookToWishList(wishListId, bookId);
        if (result.IsFaulted) 
            ModelState.AddModelError(string.Empty, $"Error adding book {bookId} to wish list {wishListId}.");
        
        var bookResult = await _bookService.GetBook(bookId);
        var book = bookResult.Match(b => b, err =>
        {
            ModelState.AddModelError(string.Empty, $"Error loading book: {err.Message}");
            return new BookDto();
        });
        
        var vm = await BuildDetailVmAsync(book);
        return RedirectToAction(nameof(Detail), new { id = bookId});
    }

    private async Task<BookDetailViewModel> BuildDetailVmAsync(BookDto book)
    {
    var email = User.Identity?.Name;
    UserDto? currentUser = null;
    if (!string.IsNullOrEmpty(email) && email != "unknown")
    {
        var userResult = await _userService.GetUserByEmailAsync(email);
        currentUser = userResult.Match(u => u, _ => null);
    }

    var reviewsResult = await _reviewService.GetAllReviewsByBookId(book.Id);
    var reviews = reviewsResult.Match(r => r, _ => new List<ReviewDto>());

    var authors = (await _authorService.GetAuthorsByIdsAsync(book.AuthorsIds ?? new()))
        .Match(a => a, _ => new List<AuthorDto>());

    var genres = (await _genreService.GetGenresByIdsAsync(book.GenresIds ?? new()))
        .Match(g => g, _ => new List<GenreDto>());
    
    var pubResult = await _publisherService.GetPublisherAsync(book.PublisherId);
    var publisherName = pubResult.Match(p => p.Name, _ => "");
    

    List<SelectListItem> wishLists = new();
    if (currentUser != null)
    {
        var wlResult = await _wishListService.GetWishListsByUserId(currentUser.Id);
        wishLists = wlResult.Match(
            lists => lists.Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.Name }).ToList(),
            _ => new List<SelectListItem>()
        );
    }

    return new BookDetailViewModel
    {
        Book = book,
        WishLists = wishLists,
        AuthorsText = string.Join(", ", authors.Select(a => a.Name)),
        GenresText = string.Join(", ", genres.Select(g => g.Name)),
        PublisherText = publisherName,
        Reviews = reviews,
        NewReview = new AddReviewViewModel { BookId = book.Id },
        CurrentUserId = currentUser?.Id ?? 0
    };

    }
    public async Task<IActionResult> Detail(int id)
    {
        var result = await _bookService.GetBook(id);
        var book = result.Match(b => b, _ => null);

        if (book == null) return NotFound();

    
        var vm = await BuildDetailVmAsync(book);
    

        return View(vm);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddReview([Bind(Prefix = "NewReview")] AddReviewViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Detail", new { id = model.BookId });
        }

        var email = User.Identity!.Name!;
        var userResult = await _userService.GetUserByEmailAsync(email);
        var user = userResult.Match(u => u, _ => null!);

        var createReviewDto = new CreateReviewDto
        {
            BookId = model.BookId,
            UserId = user.Id,
            Rating = model.Rating,
            Comment = model.Comment
        };

        await _reviewService.AddReview(createReviewDto);

        return RedirectToAction("Detail", new { id = model.BookId });
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateReview(int id, int bookId, Rating rating, string comment)
    {
        var updateDto = new UpdateReviewDto 
        { 
            Rating = rating,
            Comment = comment 
        };

        await _reviewService.UpdateReview(id, updateDto);

        return RedirectToAction("Detail", new { id = bookId });
    }
}
