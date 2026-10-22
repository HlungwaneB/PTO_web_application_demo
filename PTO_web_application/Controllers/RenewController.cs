using Microsoft.AspNetCore.Mvc;
using PTO_web_application.Models.ViewModels;
using PTO_web_application.Services;

namespace PTO_web_application.Controllers;

public class RenewController : Controller
{
    private readonly ApplicationService _applicationService;

    public RenewController(ApplicationService applicationService)
    {
        _applicationService = applicationService;
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
    public IActionResult Index(RenewPtoViewModel model)
    {
        // TODO: Save renewal request to database

        // Redirect to the success screen
        HttpContext.Session.SetString("app_reference", "PTO-RNW-" + DateTime.Now.ToString("yyyyMMdd"));
        return RedirectToAction("Submitted", "Apply");
    }
}