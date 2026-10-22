using Microsoft.AspNetCore.Mvc;
using PTO_web_application.Extensions;
using PTO_web_application.Models;
using PTO_web_application.Models.ViewModels;
using PTO_web_application.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PTO_web_application.Controllers;

public class ApplyController : Controller
{
    private readonly ApplicationService _applicationService;
    private readonly AuthService _authService;
    private readonly SiteService _siteService;
    private readonly Supabase.Client _supabase;

    public ApplyController(
        ApplicationService applicationService,
        AuthService authService,
        SiteService siteService,
        Supabase.Client supabase)
    {
        _applicationService = applicationService;
        _authService = authService;
        _siteService = siteService;
        _supabase = supabase;
    }

    // GET: /Apply  → redirect to first step
    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction("Reliable");
    }

    // ═════════════════════════════════════════════
    // RELIABLE INFORMATION
    // ═════════════════════════════════════════════
    [HttpGet]
    public IActionResult Reliable()
    {
        var saved = HttpContext.Session.GetObject<ApplyReliableViewModel>("apply_reliable")
                    ?? new ApplyReliableViewModel();
        return View(saved);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reliable(ApplyReliableViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        HttpContext.Session.SetObject("apply_reliable", model);
        return RedirectToAction("Personal");
    }

    // ═════════════════════════════════════════════
    // PERSONAL INFORMATION
    // ═════════════════════════════════════════════
    [HttpGet]
    public IActionResult Personal()
    {
        var saved = HttpContext.Session.GetObject<ApplyPersonalViewModel>("apply_personal")
                    ?? new ApplyPersonalViewModel();
        return View(saved);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Personal(ApplyPersonalViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Fail-safe validation for ID and DOB match
        if (model.DateOfBirth.HasValue && model.IdNumber.Length == 13)
        {
            string yy = model.IdNumber.Substring(0, 2);
            string mm = model.IdNumber.Substring(2, 2);
            string dd = model.IdNumber.Substring(4, 2);

            int yearShort = int.Parse(yy);
            int yearFull = yearShort <= 25 ? 2000 + yearShort : 1900 + yearShort;

            try
            {
                var dobFromId = new DateTime(yearFull, int.Parse(mm), int.Parse(dd)).Date;
                if (model.DateOfBirth.Value.Date != dobFromId)
                {
                    ModelState.AddModelError("DateOfBirth", $"Date of Birth does not match the ID Number (Expected: {dobFromId:yyyy-MM-dd}).");
                    return View(model);
                }
            }
            catch
            {
                ModelState.AddModelError("IdNumber", "Invalid ID Number format.");
                return View(model);
            }
        }

        HttpContext.Session.SetObject("apply_personal", model);
        return RedirectToAction("Contact");
    }

    // ═════════════════════════════════════════════
    // CONTACT INFORMATION & SITE SELECTION
    // ═════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Contact()
    {
        var saved = HttpContext.Session.GetObject<ApplyContactViewModel>("apply_contact")
                    ?? new ApplyContactViewModel();

        // Fetch sites from database for the dropdown
        var sites = await _siteService.GetAvailableSitesAsync();
        saved.AvailableSites = sites.Select(s => new SelectListItem
        {
            Value = s.SiteId.ToString(),
            Text = $"{s.SiteName} - {s.Location}"
        }).ToList();

        return View(saved);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ApplyContactViewModel model)
    {
        // Re-populate sites if validation fails
        var sites = await _siteService.GetAvailableSitesAsync();
        model.AvailableSites = sites.Select(s => new SelectListItem
        {
            Value = s.SiteId.ToString(),
            Text = $"{s.SiteName} - {s.Location}"
        }).ToList();

        if (!ModelState.IsValid) return View(model);

        // Check Cellphone format
        if (!System.Text.RegularExpressions.Regex.IsMatch(model.Cellphone, @"^0\d{9}$"))
            ModelState.AddModelError("Cellphone", "Please enter a valid 10-digit SA cellphone number.");

        if (model.Cellphone != model.ConfirmCellphone)
            ModelState.AddModelError("ConfirmCellphone", "Cellphone numbers do not match.");

        if (model.EmailAddress != model.ConfirmEmailAddress)
            ModelState.AddModelError("ConfirmEmailAddress", "Email addresses do not match.");

        if (!ModelState.IsValid) return View(model);

        HttpContext.Session.SetObject("apply_contact", model);
        return RedirectToAction("Dependent");
    }

    // ═════════════════════════════════════════════
    // DEPENDENT INFORMATION
    // ═════════════════════════════════════════════
    [HttpGet]
    public IActionResult Dependent()
    {
        var saved = HttpContext.Session.GetObject<ApplyDependentViewModel>("apply_dependent")
                    ?? new ApplyDependentViewModel();
        return View(saved);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Dependent(ApplyDependentViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (string.IsNullOrEmpty(model.IdNumber) || !System.Text.RegularExpressions.Regex.IsMatch(model.IdNumber, @"^\d{13}$"))
            ModelState.AddModelError("IdNumber", "ID Number must be exactly 13 digits.");

        if (string.IsNullOrEmpty(model.Cellphone) || !System.Text.RegularExpressions.Regex.IsMatch(model.Cellphone, @"^0\d{9}$"))
            ModelState.AddModelError("Cellphone", "Cellphone must be a valid 10-digit number.");

        if (!ModelState.IsValid) return View(model);

        HttpContext.Session.SetObject("apply_dependent", model);
        return RedirectToAction("Documents");
    }

    // ═════════════════════════════════════════════
    // DOCUMENTS
    // ═════════════════════════════════════════════
    [HttpGet]
    public IActionResult Documents()
    {
        return View(new ApplyDocumentsViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Documents(ApplyDocumentsViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        const long maxFileSize = 5 * 1024 * 1024;

        foreach (var file in new[] { model.CertifiedId, model.ProofOfIncome, model.DependentId })
        {
            if (file != null && file.Length > 0)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                    ModelState.AddModelError(string.Empty, $"File type not allowed for {file.FileName}.");
                if (file.Length > maxFileSize)
                    ModelState.AddModelError(string.Empty, $"File {file.FileName} is too large (Max 5MB).");
            }
        }

        if (!ModelState.IsValid) return View(model);

        async Task<string> UploadToSupabaseAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0) return string.Empty;

            try
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var fileBytes = ms.ToArray();
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                await _supabase.Storage.From("pto-documents").Upload(fileBytes, fileName);
                return _supabase.Storage.From("pto-documents").GetPublicUrl(fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SUPABASE UPLOAD ERROR: {ex.Message}");
                return "upload-failed";
            }
        }

        var idUrl = await UploadToSupabaseAsync(model.CertifiedId, "certified_ids");
        var incomeUrl = await UploadToSupabaseAsync(model.ProofOfIncome, "proof_of_income");
        var depUrl = await UploadToSupabaseAsync(model.DependentId, "dependent_ids");

        HttpContext.Session.SetString("doc_certified_id_url", idUrl);
        HttpContext.Session.SetString("doc_proof_income_url", incomeUrl);
        HttpContext.Session.SetString("doc_dependent_id_url", depUrl);

        return RedirectToAction("CreatePassword");
    }

    // ═════════════════════════════════════════════
    // CREATE PASSWORD
    // ═════════════════════════════════════════════
    [HttpGet]
    public IActionResult CreatePassword()
    {
        return View(new ApplyCreatePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreatePassword(ApplyCreatePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
            return View(model);
        }

        HttpContext.Session.SetObject("apply_password", model);
        return RedirectToAction("Review");
    }

    // ═════════════════════════════════════════════
    // REVIEW
    // ═════════════════════════════════════════════
    [HttpGet]
    public IActionResult Review()
    {
        return View(BuildReviewModel());
    }

    // ═════════════════════════════════════════════
    // SUBMIT (Saves to DB, Auth, and creates Notification)
    // ═════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit()
    {
        var personal = HttpContext.Session.GetObject<ApplyPersonalViewModel>("apply_personal");
        var contact = HttpContext.Session.GetObject<ApplyContactViewModel>("apply_contact");
        var passwordStep = HttpContext.Session.GetObject<ApplyCreatePasswordViewModel>("apply_password");

        var idUrl = HttpContext.Session.GetString("doc_certified_id_url");
        var incomeUrl = HttpContext.Session.GetString("doc_proof_income_url");
        var depUrl = HttpContext.Session.GetString("doc_dependent_id_url");

        if (personal == null || contact == null || passwordStep == null)
        {
            ViewData["ErrorMessage"] = "Session expired. Please fill out the form again.";
            return View("Review", BuildReviewModel());
        }

        try
        {
            // 1. Create Auth User
            var authSuccess = await _authService.CreatePasswordAsync(personal.IdNumber, passwordStep.Password);
            if (!authSuccess) throw new Exception("Could not create user. This ID Number might already be registered.");

            // 2. Map to DB Model using SelectedSiteId
            var newApplication = new Models.Application
            {
                IdNumber = personal.IdNumber,
                FullName = $"{personal.FirstNames} {personal.Surname}",
                PhoneNumber = contact.Cellphone,
                Email = contact.EmailAddress,
                SiteId = Guid.Parse(contact.SelectedSiteId),
                PtoType = "Residential",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1),
                CurrentStatus = "Verifying", // Changed to Verifying as requested earlier
                IsActive = true,
                CertifiedIdUrl = idUrl,
                ProofOfIncomeUrl = incomeUrl,
                DependentIdUrl = depUrl
            };

            // 3. Save to DB
            var savedApp = await _applicationService.SubmitAsync(newApplication);
            if (savedApp == null) throw new Exception("Failed to insert into the database.");

            // 4. Create the Notification
            try
            {
                var email = $"{personal.IdNumber.Trim()}@pto.local";
                var session = await _supabase.Auth.SignIn(email, passwordStep.Password);

                if (session != null && session.User != null)
                {
                    var newNotif = new Models.Notification
                    {
                        NotificationId = Guid.NewGuid(),
                        UserId = Guid.Parse(session.User.Id),
                        Message = $"Welcome! Your PTO application has been submitted successfully.",
                        DateSent = DateTime.UtcNow,
                        IsRead = false
                    };

                    await _supabase.From<Models.Notification>().Insert(newNotif);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notification failed: {ex.Message}");
            }

            // 5. Success
            HttpContext.Session.Clear();
            HttpContext.Session.SetString("app_reference", personal.IdNumber);
            return RedirectToAction("Submitted");
        }
        catch (Exception ex)
        {
            ViewData["ErrorMessage"] = $"Database Error: {ex.Message}";
            return View("Review", BuildReviewModel());
        }
    }

    // ═════════════════════════════════════════════
    // SUBMITTED (Success Screen)
    // ═════════════════════════════════════════════
    [HttpGet]
    public IActionResult Submitted()
    {
        var reference = HttpContext.Session.GetString("app_reference");
        if (string.IsNullOrEmpty(reference)) return RedirectToAction("Index", "Home");

        var model = new ApplicationSubmittedViewModel { ReferenceNumber = reference };
        return View(model);
    }

    // Helper method to rebuild the Review model if an error occurs
    private ApplyReviewViewModel BuildReviewModel()
    {
        var personal = HttpContext.Session.GetObject<ApplyPersonalViewModel>("apply_personal");
        var contact = HttpContext.Session.GetObject<ApplyContactViewModel>("apply_contact");

        return new ApplyReviewViewModel
        {
            IdNumber = personal?.IdNumber ?? "",
            Title = personal?.Title ?? "",
            Initials = personal?.Initials ?? "",
            Surname = personal?.Surname ?? "",
            Cellphone = contact?.Cellphone ?? "",
            Email = contact?.EmailAddress ?? "",
            SiteName = "Selected Site" // Placeholder for review
        };
    }
}