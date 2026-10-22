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
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        var idNumber = HttpContext.Session.GetString("sb-id-number");
        if (string.IsNullOrEmpty(idNumber))
        {
            return RedirectToAction("Login", "Auth");
        }

        // The form only posts the fields of the tab that is open, so only check that tab's fields
        var tab = (model.ActiveTab ?? "personal").ToLower();
        var relevant = tab switch
        {
            "personal" => new[] { nameof(model.PrimaryMobile), nameof(model.PrimaryEmail) },
            "address" => new[] { nameof(model.StreetNumber), nameof(model.Suburb), nameof(model.PostalAddress), nameof(model.PostalCode) },
            "nextofkin" => new[] { nameof(model.KinName), nameof(model.KinSurname), nameof(model.KinPhone), nameof(model.KinEmail) },
            _ => Array.Empty<string>()
        };
        var hasError = ModelState.Where(kv => relevant.Contains(kv.Key) && kv.Value!.Errors.Count > 0).Any();

        if (tab == "personal")
        {
            if (string.IsNullOrWhiteSpace(model.PrimaryMobile)) { ModelState.AddModelError(nameof(model.PrimaryMobile), "Mobile number is required."); hasError = true; }
            if (string.IsNullOrWhiteSpace(model.PrimaryEmail)) { ModelState.AddModelError(nameof(model.PrimaryEmail), "Email address is required."); hasError = true; }
        }

        if (hasError)
        {
            // Show the form again, still in edit mode, with the messages
            var application = await _applicationService.GetMyApplicationAsync(idNumber);
            model.IsEditing = true;
            model.IdentityNumber = idNumber; // never trust a posted ID number
            model.FullName = HttpContext.Session.GetString("sb-full-name") ?? "User";
            model.Name = application?.FullName?.Split(' ').FirstOrDefault() ?? "";
            model.Surname = application?.FullName?.Split(' ').LastOrDefault() ?? "";
            var parts = model.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            model.Initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                : model.FullName.Substring(0, Math.Min(2, model.FullName.Length)).ToUpper();
            return View(model);
        }

        // Save the two contact details that exist in the applications table
        if (tab == "personal")
        {
            try
            {
                await _applicationService.UpdateContactAsync(
                    idNumber,
                    PTO_web_application.Validation.Rules.NormalizePhone(model.PrimaryMobile),
                    model.PrimaryEmail.Trim().ToLowerInvariant());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Profile update failed: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Your details could not be saved. Please try again.");
                model.IsEditing = true;
                model.IdentityNumber = idNumber;
                model.FullName = HttpContext.Session.GetString("sb-full-name") ?? "User";
                model.Initials = "U";
                return View(model);
            }
        }

        // Redirect back to view mode
        return RedirectToAction("Index", new { tab = model.ActiveTab });
    }
}