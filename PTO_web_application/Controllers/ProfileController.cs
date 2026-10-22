using Microsoft.AspNetCore.Mvc;
using PTO_web_application.Models.ViewModels;
using PTO_web_application.Services;

namespace PTO_web_application.Controllers;

public class ProfileController : Controller
{
    private readonly ApplicationService _applicationService;

    public ProfileController(ApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string tab = "personal", bool isEditing = false)
    {
        var idNumber = HttpContext.Session.GetString("sb-id-number");
        if (string.IsNullOrEmpty(idNumber))
        {
            return RedirectToAction("Login", "Auth");
        }

        // Fetch real data from database
        var application = await _applicationService.GetMyApplicationAsync(idNumber);
        var fullName = HttpContext.Session.GetString("sb-full-name") ?? "User";

        var model = new ProfileViewModel
        {
            IsEditing = isEditing, // 🟢 Set edit state
            FullName = fullName,
            ActiveTab = tab.ToLower(),

            IdentityNumber = idNumber,
            Name = application?.FullName?.Split(' ').FirstOrDefault() ?? "",
            Surname = application?.FullName?.Split(' ').LastOrDefault() ?? "",
            PrimaryMobile = application?.PhoneNumber ?? "",
            PrimaryEmail = application?.Email ?? ""
        };

        // Get initials for the large avatar
        var parts = model.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        model.Initials = parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
            : model.FullName.Substring(0, Math.Min(2, model.FullName.Length)).ToUpper();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(ProfileViewModel model)
    {
        // TODO: Save to Supabase later

        // Redirect back to view mode
        return RedirectToAction("Index", new { tab = model.ActiveTab });
    }
}