using System.Diagnostics;
using BusinessLayer.Dto.Author;
using BusinessLayer.Dto.Book;
using BusinessLayer.Dto.Genre;
using BusinessLayer.Dto.Publisher;
using BusinessLayer.Service;
using BookHub.Mvc.Models;
using BookHub.Mvc.Models.User;
using BookHub.Mvc.Models.User.Home;
using BusinessLayer.Dto.Cart;
using BusinessLayer.Dto.User;
using BusinessLayer.Dto.Wishlist;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookHub.Mvc.Controller;

public class HomeController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly IBookService _bookService;
    private readonly IGlobalSearchService _globalSearchService;
    private readonly IAuthorService _authorService;
    private readonly IGenreService _genreService;
    private readonly IPublisherService _publisherService;
    private readonly ICartService _cartService;
    private readonly IUserService _userService;
    private readonly IWishListService _wishListService;

    public HomeController(
        IBookService bookService,
        IGlobalSearchService globalSearchService,
        IAuthorService authorService,
        IGenreService genreService,
        IPublisherService publisherService,
        IUserService userService,
        ICartService cartService,
        IWishListService wishListService)
    {
        _bookService = bookService;
        _globalSearchService = globalSearchService;
        _authorService = authorService;
        _genreService = genreService;
        _publisherService = publisherService;
        _cartService = cartService;
        _userService = userService;
        _wishListService = wishListService;
    }

    public async Task<IActionResult> Index(string? q)
    {
        ViewData["q"] = q ?? "";

        var books = new List<BookDto>();

        if (string.IsNullOrWhiteSpace(q))
        {
            var allResult = await _bookService.GetBooks(options: null, includeImages: true);
            books = allResult.Match(
                Succ: b => b ?? new List<BookDto>(),
                Fail: ex =>
                {
                    ViewData["Error"] = ex.Message;
                    return new List<BookDto>();
                }
            );
        }
        else
        {
            var term = q.Trim();

            var search = await _globalSearchService.SearchAll(term);
            var searchBooksResult = _bookService.GetBooksByIdsAsync(search.Books.Select(b => b.Id), true);
            books.AddRange(
                searchBooksResult.Result.Match(
                    Succ: b => b ?? [],
                    Fail: ex =>
                    {
                        ViewData["Error"] = ex.Message;
                        return new List<BookDto>();
                    }));
        }

        var vm = await BuildHomeListVmAsync(books);
        return View(vm);
    }


    [HttpGet]
    public async Task<IActionResult> Suggest(string? term)
    {
        term = (term ?? "").Trim();
        if (term.Length < 2)
        {
            return Ok(new SuggestResponse());
        }

        var results = await _globalSearchService.SearchAll(term);

        var response = new SuggestResponse
        {
            Books = results.Books.Select(b => new SuggestItem
            {
                Type = "book",
                Id = b.Id,
                Label = b.Name
            }).ToList(),

            Genres = results.Genres.Select(g => new SuggestItem
            {
                Type = "genre",
                Id = g.Id,
                Label = g.Name
            }).ToList(),

            Publishers = results.Publishers.Select(p => new SuggestItem
            {
                Type = "publisher",
                Id = p.Id,
                Label = p.Name
            }).ToList()
        };

        return Ok(response);
    }

    public async Task<IActionResult> AddToCartFromHome(int id)
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


        var allResult = await _bookService.GetBooks(options: null, includeImages: true);
        var books = allResult.Match(
            Succ: b => b ?? new List<BookDto>(),
            Fail: ex =>
            {
                ViewData["Error"] = ex.Message;
                return new List<BookDto>();
            }
        );

        var vm = await BuildHomeListVmAsync(books);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> AddToWishListFromHome(int bookId, int wishListId)
    {
        var result = await _wishListService.AddBookToWishList(wishListId, bookId);
        if (result.IsFaulted)
            ModelState.AddModelError(string.Empty, $"Error adding book {bookId} to wish list {wishListId}.");

        var allResult = await _bookService.GetBooks(options: null, includeImages: true);
        var books = allResult.Match(
            Succ: b => b ?? new List<BookDto>(),
            Fail: ex =>
            {
                ViewData["Error"] = ex.Message;
                return new List<BookDto>();
            }
        );
        var vm = await BuildHomeListVmAsync(books);
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<HomeBookListItemViewModel>> BuildHomeListVmAsync(List<BookDto> books)
    {
        var authorsResult = await _authorService.GetAuthorsAsync(null);
        var authors = authorsResult.Match(
            succ => succ,
            _ => new List<AuthorDto>());

        var genresResult = await _genreService.GetGenres(null);
        var genres = genresResult.Match(
            succ => succ,
            _ => new List<GenreDto>());

        var publishersResult = await _publisherService.GetPublishers(null!);
        var publishers = publishersResult.Match(
            succ => succ,
            _ => new List<PublisherDto>());

        var authorById = authors.ToDictionary(a => a.Id, a => a.Name);
        var genreById = genres.ToDictionary(g => g.Id, g => g.Name);
        var publisherById = publishers.ToDictionary(p => p.Id, p => p.Name);

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

        var wishLists = wishListsDto.Select(wl => new SelectListItem
        {
            Value = wl.Id.ToString(),
            Text = wl.Name
        });

        return books.Select(b =>
        {
            var authorNames = (b.AuthorsIds ?? new List<int>())
                .Where(authorById.ContainsKey)
                .Select(id => authorById[id]);

            var genreNames = (b.GenresIds ?? new List<int>())
                .Where(genreById.ContainsKey)
                .Select(id => genreById[id]);

            var publisherName = publisherById.TryGetValue(b.PublisherId, out var pn) ? pn : "";

            return new HomeBookListItemViewModel
            {
                Book = b,
                WishLists = wishLists,
                AuthorsText = string.Join(", ", authorNames),
                GenresText = string.Join(", ", genreNames),
                PublisherText = publisherName
            };
        }).ToList();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}