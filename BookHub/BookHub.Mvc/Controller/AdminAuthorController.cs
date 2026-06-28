using BookHub.Mvc.Models.Admin;
using BusinessLayer.Dto.Author;
using BusinessLayer.Service;
using LanguageExt.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHub.Mvc.Controller;

[Authorize(Roles = "Admin")]
public class AdminAuthorController : Microsoft.AspNetCore.Mvc.Controller
{

    private readonly IAuthorService _authorService;

    public AdminAuthorController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    public async Task<IActionResult> Index()
    {
        Result<List<AuthorDto>> result = await _authorService.GetAuthorsAsync(null);

        if (result.IsFaulted)
        {
            var error = result.Match(_ => null, ex => ex.Message);
            ViewData["Error"] = error;
            return View( Enumerable.Empty<AuthorDto>());
        }

        var authors = result.Match(list => list, _ => new List<AuthorDto>());
        return View(authors);
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await _authorService.GetAuthorByIdAsync(id);
        if (result.IsFaulted)
            return NotFound();

        var author = result.Match(a => a, _ => null);
        return View(author!);
    }

    public IActionResult Create()
    {
        return View(new AdminAuthorFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminAuthorFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var req = new AddAuthorDto
        {
            Name = vm.Name
        };

        var result = await _authorService.AddAuthor(req);

        var error = result.Match<string?>(
            _ => null,
            ex => ex.Message);

        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _authorService.GetAuthorByIdAsync(id);
        if (result.IsFaulted)
            return NotFound();

        var dto = result.Match(a => a, _ => null)!;

        var vm = new AdminAuthorFormViewModel
        {
            Id = dto.Id,
            Name = dto.Name
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminAuthorFormViewModel vm)
    {
        if (id != vm.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(vm);

        var req = new UpdateAuthorDto
        {
            Name = vm.Name
        };

        var result = await _authorService.UpdateAuthor(id, req);

        var error = result.Match<string?>(
            _ => null,
            ex => ex.Message);

        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var result = await _authorService.GetAuthorByIdAsync(id);
        if (result.IsFaulted)
            return NotFound();

        var dto = result.Match(a => a, _ => null)!;

        var canDeleteResult = await _authorService.CanDeleteAuthorAsync(id);
        var canDelete = canDeleteResult.Match(x => x, _ => false);
        ViewData["CanDelete"] = canDelete;

        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleteResult = await _authorService.DeleteAuthorAsync(id);

        var error = deleteResult.Match<string?>(
            _ => null,
            ex => ex.Message);

        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);

            var dtoResult = await _authorService.GetAuthorByIdAsync(id);
            if (dtoResult.IsFaulted)
                return NotFound();

            var dto = dtoResult.Match(a => a, _ => null)!;

            var canDeleteResult = await _authorService.CanDeleteAuthorAsync(id);
            ViewData["CanDelete"] = canDeleteResult.Match(x => x, _ => false);

            return View(dto);
        }

        return RedirectToAction(nameof(Index));
    }
}