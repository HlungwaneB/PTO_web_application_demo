using Microsoft.AspNetCore.Mvc;
using PTO_web_application.Models;
using PTO_web_application.Models.ViewModels;
using PTO_web_application.Services;

namespace PTO_web_application.Controllers;

public class DashboardController : Controller
{
    private readonly ApplicationService _applicationService;
    private readonly SiteService _siteService;

    public DashboardController(ApplicationService applicationService, SiteService siteService)
    {
        _applicationService = applicationService;
        _siteService = siteService;
    }

    public async Task<IActionResult> Index()
    {
        var idNumber = HttpContext.Session.GetString("sb-id-number");
        if (string.IsNullOrEmpty(idNumber))
        {
            return RedirectToAction("Login", "Auth");
        }

        var application = await _applicationService.GetMyApplicationAsync(idNumber);

        Site? site = null;
        if (application != null)
        {
            site = await _siteService.GetSiteByIdAsync(application.SiteId);
        }

        // 🟢 Tell the Layout to show the correct navigation tabs
        ViewData["HasApplication"] = application != null;

        var model = new DashboardViewModel
        {
            FullName = application?.FullName ?? HttpContext.Session.GetString("sb-full-name") ?? "User",
            IdNumber = idNumber,
            CurrentApplication = application,
            CurrentSite = site
        };

        var parts = model.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        model.Initials = parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
            : model.FullName.Substring(0, Math.Min(2, model.FullName.Length)).ToUpper();

        return View(model);
    }

    // ═════════════════════════════════════════════
    // TRACKING TIMELINE
    // ═════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Tracking()
    {
        var idNumber = HttpContext.Session.GetString("sb-id-number");
        if (string.IsNullOrEmpty(idNumber))
        {
            return RedirectToAction("Login", "Auth");
        }

        var application = await _applicationService.GetMyApplicationAsync(idNumber);

        // 🟢 Tell the Layout to show the correct navigation tabs
        ViewData["HasApplication"] = application != null;

        ViewBag.CurrentStatus = application?.CurrentStatus ?? "Pending";

        return View();
    }
}