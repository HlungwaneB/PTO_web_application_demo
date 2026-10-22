using Microsoft.AspNetCore.Mvc;
using PTO_web_application.Models.ViewModels;
using PTO_web_application.Services;

namespace PTO_web_application.Controllers;

public class AuthController : Controller
{
    private readonly AuthService _authService;
    private readonly ApplicationService _applicationService;

    public AuthController(AuthService authService, ApplicationService applicationService)
    {
        _authService = authService;
        _applicationService = applicationService;
    }

    // ─────────────────────────────────────────────
    // GET: /Auth/Login
    // ─────────────────────────────────────────────
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // ─────────────────────────────────────────────
    // POST: /Auth/Login
    // ─────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // 1. Authenticate with Supabase
        var session = await _authService.LoginAsync(model.IdNumber, model.Password);

        if (session == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid ID number or password.");
            return View(model);
        }

        // 2. Store session data
        var email = session.User?.Email ?? "";
        HttpContext.Session.SetString("sb-token", session.AccessToken ?? string.Empty);
        HttpContext.Session.SetString("sb-refresh-token", session.RefreshToken ?? string.Empty);
        HttpContext.Session.SetString("sb-user-id", session.User?.Id ?? string.Empty);
        HttpContext.Session.SetString("sb-user-email", email);
        HttpContext.Session.SetString("sb-id-number", model.IdNumber.Trim());

        // 3. Fetch the full name for the layout (Admin gets a special name)
        if (model.IdNumber.Trim().ToLower() == "admin")
        {
            HttpContext.Session.SetString("sb-full-name", "System Admin");
        }
        else
        {
            var application = await _applicationService.GetMyApplicationAsync(model.IdNumber);
            HttpContext.Session.SetString("sb-full-name", application?.FullName ?? "User");
        }

        // 4. Redirect based on user role
        if (model.IdNumber.Trim().ToLower() == "admin")
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    // ─────────────────────────────────────────────
    // POST: /Auth/Logout
    // ─────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}