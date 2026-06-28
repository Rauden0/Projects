using BusinessLayer.Dto.Author;
using BusinessLayer.Dto.Book;
using BusinessLayer.Dto.Genre;
using BusinessLayer.Dto.Publisher;
using BusinessLayer.Service;
using BookHub.Mvc.Models.Admin;
using BusinessLayer.Service.Logging;
using LanguageExt.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookHub.Mvc.Controller;

[Authorize(Roles = "Admin")]
public class AdminBookController : Microsoft.AspNetCore.Mvc.Controller
{

    private readonly IBookService _bookService;
    private readonly IAuthorService _authorService;
    private readonly IGenreService _genreService;
    private readonly IPublisherService _publisherService;
    private readonly IAuditLogService _auditLogService;

    public AdminBookController(
        IBookService bookService,
        IAuthorService authorService,
        IGenreService genreService,
        IPublisherService publisherService,
        IAuditLogService auditLogService)
    {
        _bookService = bookService;
        _authorService = authorService;
        _genreService = genreService;
        _publisherService = publisherService;
        _auditLogService = auditLogService;
    }

    private async Task FillSelectListsAsync(AdminBookFormViewModel vm)
    {
        var authorsResult = await _authorService.GetAuthorsAsync(null);
        var authors = authorsResult.Match(
            succ => succ,
            err =>
            {
                ModelState.AddModelError(string.Empty, $"Error loading authors: {err.Message}");
                return new List<AuthorDto>();
            });

        var genresResult = await _genreService.GetGenres(null);
        var genres = genresResult.Match(
            succ => succ,
            err =>
            {
                ModelState.AddModelError(string.Empty, $"Error loading genres: {err.Message}");
                return new List<GenreDto>();
            });

        var publishersResult = await _publisherService.GetPublishers(null!);
        var publishers = publishersResult.Match(
            succ => succ,
            err =>
            {
                ModelState.AddModelError(string.Empty, $"Error loading publishers: {err.Message}");
                return new List<PublisherDto>();
            });

        vm.Authors = authors.Select(a => new SelectListItem
        {
            Value = a.Id.ToString(),
            Text = a.Name
        });

        vm.Genres = genres.Select(g => new SelectListItem
        {
            Value = g.Id.ToString(),
            Text = g.Name
        });

        vm.Publishers = publishers.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = p.Name
        });
    }

    public async Task<IActionResult> Index()
    {
        Result<List<BookDto>> result = await _bookService.GetBooks(null);

        if (result.IsFaulted)
        {
            var error = result.Match(_ => null, ex => ex.Message);
            ViewData["Error"] = error;
            return View(Enumerable.Empty<BookDto>());
        }

        var books = result.Match(list => list, _ => new List<BookDto>());
        return View(books);
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await _bookService.GetBook(id);
        if (result.IsFaulted)
        {
            return NotFound();
        }

        var book = result.Match(b => b, _ => null);
        return View(book!);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new AdminBookFormViewModel();
        await FillSelectListsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminBookFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await FillSelectListsAsync(vm);
            return View(vm);
        }

        var request = new RequestBookDto
        {
            AuthorIds = vm.SelectedAuthorIds,
            GenreIds = vm.SelectedGenreIds,
            PublisherId = vm.PublisherId,
            Name = vm.Name,
            Description = vm.Description,
            Price = vm.Price,
            StockQuantity = vm.StockQuantity,
            Image = vm.Image
        };

        var result = await _bookService.AddBook(request);

        var error = result.Match<string?>(
            _ => null,
            ex => ex.Message);

        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            await FillSelectListsAsync(vm);
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _bookService.GetBook(id);
        if (result.IsFaulted)
        {
            return NotFound();
        }

        var dto = result.Match(b => b, _ => null)!;

        var vm = new AdminBookFormViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            SelectedAuthorIds = dto.AuthorsIds ?? new List<int>(),
            SelectedGenreIds = dto.GenresIds ?? new List<int>(),
            PublisherId = dto.PublisherId
        };

        await FillSelectListsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminBookFormViewModel vm)
    {
        if (id != vm.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await FillSelectListsAsync(vm);
            return View(vm);
        }

        var request = new RequestBookDto
        {
            AuthorIds = vm.SelectedAuthorIds,
            GenreIds = vm.SelectedGenreIds,
            PublisherId = vm.PublisherId,
            Name = vm.Name,
            Description = vm.Description,
            Price = vm.Price,
            StockQuantity = vm.StockQuantity,
            Image = vm.Image
        };

        var result = await _bookService.UpdateBook(id, request);
        
        var user = User.Identity?.Name ?? "unknown";
        await _auditLogService.LogAsync(user, "Book", id.ToString(), "Edit", result.IsSuccess.ToString());

        var error = result.Match<string?>(
            _ => null,
            ex => ex.Message);

        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            await FillSelectListsAsync(vm);
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var result = await _bookService.GetBook(id);
        if (result.IsFaulted)
        {
            return NotFound();
        }

        var dto = result.Match(b => b, _ => null)!;
        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _bookService.DeleteBook(id);
        return RedirectToAction(nameof(Index));
    }
}
