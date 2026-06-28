using BookHub.Mvc.Models.Admin;
using BusinessLayer.Dto.Genre;
using BusinessLayer.Service;
using LanguageExt.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHub.Mvc.Controller;

[Authorize(Roles = "Admin")]
public class AdminGenreController : Microsoft.AspNetCore.Mvc.Controller
{

    private readonly IGenreService _genreService;

    public AdminGenreController(IGenreService genreService)
    {
        _genreService = genreService;
    }

    public async Task<IActionResult> Index()
    {
        Result<List<GenreDto>> result = await _genreService.GetGenres(null);

        if (result.IsFaulted)
        {
            var error = result.Match(_ => null, ex => ex.Message);
            ViewData["Error"] = error;
            return View(Enumerable.Empty<GenreDto>());
        }

        var genres = result.Match(list => list, _ => new List<GenreDto>());
        return View(genres);
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await _genreService.GetGenreAsync(id);
        if (result.IsFaulted)
            return NotFound();

        var dto = result.Match(g => g, _ => null);
        return View(dto!);
    }

    public IActionResult Create()
    {
        return View(new AdminGenreFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminGenreFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var req = new CreateGenreDto { Name = vm.Name };
        var result = await _genreService.CreateAsync(req);

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
        var result = await _genreService.GetGenreAsync(id);
        if (result.IsFaulted)
            return NotFound();

        var dto = result.Match(g => g, _ => null)!;

        var vm = new AdminGenreFormViewModel
        {
            Id = dto.Id,
            Name = dto.Name
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminGenreFormViewModel vm)
    {
        if (id != vm.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(vm);

        var req = new UpdateGenreDto { Name = vm.Name };
        var result = await _genreService.UpdateAsync(id, req);

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
        var result = await _genreService.GetGenreAsync(id);
        if (result.IsFaulted)
            return NotFound();

        var dto = result.Match(g => g, _ => null)!;

        var canDeleteResult = await _genreService.CanDeleteGenreAsync(id);
        ViewData["CanDelete"] = canDeleteResult.Match(x => x, _ => false);

        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleteResult = await _genreService.DeleteGenreAsync(id);

        var error = deleteResult.Match<string?>(
            _ => null,
            ex => ex.Message);

        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);

            var dtoResult = await _genreService.GetGenreAsync(id);
            if (dtoResult.IsFaulted)
                return NotFound();

            var dto = dtoResult.Match(g => g, _ => null)!;

            var canDeleteResult = await _genreService.CanDeleteGenreAsync(id);
            ViewData["CanDelete"] = canDeleteResult.Match(x => x, _ => false);

            return View(dto);
        }

        return RedirectToAction(nameof(Index));
    }
}