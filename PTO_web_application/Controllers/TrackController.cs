using Microsoft.AspNetCore.Mvc;
using PTO_web_application.Models.ViewModels;
using PTO_web_application.Services;

namespace PTO_web_application.Controllers;

public class TrackController : Controller
{
    private readonly ApplicationService _applicationService;
    private readonly SiteService _siteService;

    public TrackController(ApplicationService applicationService, SiteService siteService)
    {
        _applicationService = applicationService;
        _siteService = siteService;
    }

    // GET: /Track
    [HttpGet]
    public IActionResult Index()
    {
        return View(new TrackViewModel());
    }

    // POST: /Track
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(TrackViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.IdNumber))
        {
            ModelState.AddModelError(nameof(model.IdNumber), "Please enter an ID number");
            return View(model);
        }

        // Only a 13-digit ID number can have an application (also stops odd text reaching the query)
        if (!ModelState.IsValid) return View(model);

        var idNumber = model.IdNumber.Trim();

        // 1. Fetch the application from Supabase
        var application = await _applicationService.GetMyApplicationAsync(idNumber);

        if (application == null)
        {
            // 2. Not Found: Show the "Not Found" screen
            var nfModel = new TrackNotFoundViewModel { IdNumber = idNumber };
            return View("NotFound", nfModel);
        }

        // 3. Found: Fetch the Site name
        var site = await _siteService.GetSiteByIdAsync(application.SiteId);

        // 4. Build the Result Model
        var resultModel = new TrackResultViewModel
        {
            IdNumber = application.IdNumber,
            Title = "Applicant", // We didn't collect Title in the Apply form, so we use a default
            Initials = "",
            Surname = application.FullName.Split(' ').LastOrDefault() ?? application.FullName,
            Year = application.EndDate.Year.ToString(),
            SiteName = site?.SiteName ?? "Unknown Site",
            Status = application.CurrentStatus // This will be "Verifying" or "Approved"
        };

        return View("Result", resultModel);
    }
}