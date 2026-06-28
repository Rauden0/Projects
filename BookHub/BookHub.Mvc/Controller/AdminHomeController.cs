using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHub.Mvc.Controller;

[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminHomeController : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }
}