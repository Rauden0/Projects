using BookHub.Mvc.Models;
using BookHub.Mvc.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookHub.Mvc.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/users")]
public class AdminUserController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUserController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var users = _userManager.Users.ToList();
        return View(users);
    }

    [HttpGet("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var vm = new AdminResetPasswordViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName
        };

        return View(vm);
    }

    [HttpPost("{id}/reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id, AdminResetPasswordViewModel vm)
    {
        if (id != vm.UserId)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(vm);

        var user = await _userManager.FindByIdAsync(vm.UserId);
        if (user == null) return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(vm);
        }

        TempData["Success"] = $"Password reset for {user.Email}";
        return RedirectToAction(nameof(Index));
    }
}
