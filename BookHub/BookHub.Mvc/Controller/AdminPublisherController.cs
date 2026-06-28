using BookHub.Mvc.Models.Admin;
using BusinessLayer.Dto.Publisher;
using BusinessLayer.Service;
using LanguageExt.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHub.Mvc.Controller;

[Authorize(Roles = "Admin")]
public class AdminPublisherController : Microsoft.AspNetCore.Mvc.Controller
{

    private readonly IPublisherService _publisherService;

    public AdminPublisherController(IPublisherService publisherService)
    {
        _publisherService = publisherService;
    }

    public async Task<IActionResult> Index()
    {
        Result<List<PublisherDto>> result = await _publisherService.GetPublishers(null!);

        if (result.IsFaulted)
        {
            var error = result.Match(_ => null, ex => ex.Message);
            ViewData["Error"] = error;
            return View(Enumerable.Empty<PublisherDto>());
        }

        var publishers = result.Match(list => list, _ => new List<PublisherDto>());
        return View(publishers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await _publisherService.GetPublisherAsync(id);
        if (result.IsFaulted)
            return NotFound();

        var publisher = result.Match(p => p, _ => null);
        return View(publisher!);
    }

    public IActionResult Create()
    {
        return View(new AdminPublisherFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminPublisherFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var req = new CreatePublisherDto
        {
            Name = vm.Name
        };

        var result = await _publisherService.CreateAsync(req);

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
        var result = await _publisherService.GetPublisherAsync(id);
        if (result.IsFaulted)
            return NotFound();

        var dto = result.Match(p => p, _ => null)!;

        var vm = new AdminPublisherFormViewModel
        {
            Id = dto.Id,
            Name = dto.Name
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminPublisherFormViewModel vm)
    {
        if (id != vm.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(vm);

        var req = new UpdatePublisherDto
        {
            Name = vm.Name
        };

        var result = await _publisherService.UpdateAsync(id, req);

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
        var result = await _publisherService.GetPublisherAsync(id);
        if (result.IsFaulted)
            return NotFound();

        var dto = result.Match(p => p, _ => null)!;

        var canDeleteResult = await _publisherService.CanDeleteAsync(id);
        var canDelete = canDeleteResult.Match(x => x, _ => false);

        ViewData["CanDelete"] = canDelete;

        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleteResult = await _publisherService.DeleteAsync(id);

        var error = deleteResult.Match<string?>(
            _ => null,
            ex => ex.Message);

        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);

            var dtoResult = await _publisherService.GetPublisherAsync(id);
            if (dtoResult.IsFaulted)
                return NotFound();

            var dto = dtoResult.Match(p => p, _ => null)!;

            var canDeleteResult = await _publisherService.CanDeleteAsync(id);
            var canDelete = canDeleteResult.Match(x => x, _ => false);
            ViewData["CanDelete"] = canDelete;

            return View(dto);
        }

        return RedirectToAction(nameof(Index));
    }
}