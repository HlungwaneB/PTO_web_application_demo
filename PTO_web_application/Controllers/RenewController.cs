using Microsoft.AspNetCore.Mvc;
using PTO_web_application.Models.ViewModels;
using PTO_web_application.Services;

namespace PTO_web_application.Controllers;

public class RenewController : Controller
{
    private readonly ApplicationService _applicationService;

    private readonly Supabase.Client _supabase;

    public RenewController(ApplicationService applicationService, Supabase.Client supabase)
    {
        _applicationService = applicationService;
        _supabase = supabase;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var idNumber = HttpContext.Session.GetString("sb-id-number");
        if (string.IsNullOrEmpty(idNumber))
        {
            return RedirectToAction("Login", "Auth");
        }

        // Get the user's current application
        var application = await _applicationService.GetMyApplicationAsync(idNumber);

        var model = new RenewPtoViewModel
        {
            // Pull the expiry date from the database
            ExpiryDate = application?.EndDate
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(RenewPtoViewModel model)
    {
        var idNumber = HttpContext.Session.GetString("sb-id-number");
        if (string.IsNullOrEmpty(idNumber))
        {
            return RedirectToAction("Login", "Auth");
        }

        var application = await _applicationService.GetMyApplicationAsync(idNumber);
        model.ExpiryDate = application?.EndDate; // always take the expiry date from the database, never from the form

        if (!ModelState.IsValid) return View(model);

        // Only an approved PTO can be renewed, and only when it expires within 2 months (or already expired)
        if (application == null || application.CurrentStatus != "Approved")
        {
            ModelState.AddModelError(string.Empty, "You can only renew an approved PTO.");
            return View(model);
        }
        if (application.EndDate > DateTime.UtcNow.AddMonths(2))
        {
            ModelState.AddModelError(string.Empty, $"Renewal opens 2 months before your PTO expires ({application.EndDate:dd MMM yyyy}).");
            return View(model);
        }
        if (model.StillLivingAtSite == "No")
        {
            ModelState.AddModelError(nameof(model.StillLivingAtSite), "A PTO can only be renewed while you still live at the site. Please contact the municipality.");
            return View(model);
        }

        // Leave a notification for the applicant (uses the existing notifications table only)
        try
        {
            var userId = HttpContext.Session.GetString("sb-user-id");
            if (Guid.TryParse(userId, out var uid))
            {
                await _supabase.From<Models.Notification>().Insert(new Models.Notification
                {
                    NotificationId = Guid.NewGuid(),
                    UserId = uid,
                    Message = "Your PTO renewal request was received. You will be notified when it has been reviewed.",
                    DateSent = DateTime.UtcNow,
                    IsRead = false
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Renewal notification failed: {ex.Message}");
        }

        // Redirect to the success screen
        HttpContext.Session.SetString("app_reference", "PTO-RNW-" + DateTime.Now.ToString("yyyyMMdd"));
        return RedirectToAction("Submitted", "Apply");
    }
}