using Microsoft.AspNetCore.Mvc;
using PTO_web_application.Extensions;
using PTO_web_application.Models;
using PTO_web_application.Models.ViewModels;
using PTO_web_application.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using PTO_web_application.Validation;

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
    public async Task<IActionResult> Personal(ApplyPersonalViewModel model)
    {
        // The eligibility step must be finished first
        if (HttpContext.Session.GetObject<ApplyReliableViewModel>("apply_reliable") == null)
            return RedirectToAction("Reliable");

        if (!ModelState.IsValid) return View(model);

        // Normalise the text so " thabo " and "Thabo" are stored the same way
        model.IdNumber = model.IdNumber.Trim();
        model.Surname = model.Surname.Trim();
        model.FirstNames = model.FirstNames.Trim();
        model.Initials = model.Initials.Replace(".", "").Trim().ToUpperInvariant();

        // One application per ID number
        if (await _applicationService.IdNumberExistsAsync(model.IdNumber))
        {
            ModelState.AddModelError("IdNumber", "An application already exists for this ID Number. Please log in or track your application.");
            return View(model);
        }

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

        // Verification: one application per ID number (this only READS Supabase)
        var existing = await _applicationService.GetMyApplicationAsync(model.IdNumber.Trim());
        if (existing != null)
        {
            ModelState.AddModelError("IdNumber", "An application already exists for this ID number. Please log in or track it instead.");
            return View(model);
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

        // Earlier steps must be finished
        if (HttpContext.Session.GetObject<ApplyPersonalViewModel>("apply_personal") == null)
            return RedirectToAction("Personal");

        // Normalise: 082 123 4567 and +27821234567 become 0821234567, e-mail addresses are trimmed and lower-case
        model.Cellphone = Rules.NormalizePhone(model.Cellphone);
        model.ConfirmCellphone = Rules.NormalizePhone(model.ConfirmCellphone);
        model.HomeCellphone = Rules.NormalizePhone(model.HomeCellphone);
        model.EmailAddress = (model.EmailAddress ?? string.Empty).Trim().ToLowerInvariant();
        model.ConfirmEmailAddress = (model.ConfirmEmailAddress ?? string.Empty).Trim().ToLowerInvariant();

        // The chosen site must be a real site that is still free
        if (!Guid.TryParse(model.SelectedSiteId, out var chosenSiteId))
        {
            ModelState.AddModelError("SelectedSiteId", "Please select a site.");
        }
        else
        {
            var chosenSite = await _siteService.GetSiteByIdAsync(chosenSiteId);
            if (chosenSite == null || !chosenSite.IsAvailable)
                ModelState.AddModelError("SelectedSiteId", "That site is no longer available. Please choose another site.");
        }

        // Check Cellphone format
        if (!System.Text.RegularExpressions.Regex.IsMatch(model.Cellphone, @"^0\d{9}$"))
            ModelState.AddModelError("Cellphone", "Please enter a valid 10-digit SA cellphone number.");

        if (model.Cellphone != model.ConfirmCellphone)
            ModelState.AddModelError("ConfirmCellphone", "Cellphone numbers do not match.");

        if (model.EmailAddress != model.ConfirmEmailAddress)
            ModelState.AddModelError("ConfirmEmailAddress", "Email addresses do not match.");

        if (!ModelState.IsValid) return View(model);

        HttpContext.Session.SetObject("apply_contact", model);

        // Only people who answered YES to "Are you dependant" see the Dependent information step
        if (model.IsDependant != "Yes")
        {
            HttpContext.Session.Remove("apply_dependent");
            return RedirectToAction("Documents");
        }
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

        model.Cellphone = Rules.NormalizePhone(model.Cellphone);
        model.Initials = (model.Initials ?? string.Empty).Replace(".", "").Trim().ToUpperInvariant();

        var applicant = HttpContext.Session.GetObject<ApplyPersonalViewModel>("apply_personal");
        if (applicant == null) return RedirectToAction("Personal");
        if (applicant.IdNumber == model.IdNumber)
            ModelState.AddModelError("IdNumber", "A dependant cannot have the same ID Number as the applicant.");

        if (string.IsNullOrEmpty(model.IdNumber) || !System.Text.RegularExpressions.Regex.IsMatch(model.IdNumber, @"^\d{13}$"))
            ModelState.AddModelError("IdNumber", "ID Number must be exactly 13 digits.");

        if (string.IsNullOrEmpty(model.Cellphone) || !System.Text.RegularExpressions.Regex.IsMatch(model.Cellphone, @"^0\d{9}$"))
            ModelState.AddModelError("Cellphone", "Cellphone must be a valid 10-digit number.");

        // Verification: the dependant's ID number must be a real South African ID
        if (!string.IsNullOrEmpty(model.IdNumber) && !SaIdValidator.IsValid(model.IdNumber))
            ModelState.AddModelError("IdNumber", "This is not a valid South African ID number. Check the digits.");

        var applicant = HttpContext.Session.GetObject<ApplyPersonalViewModel>("apply_personal");
        if (applicant != null && model.IdNumber == applicant.IdNumber)
            ModelState.AddModelError("IdNumber", "A dependant cannot have the same ID number as the applicant.");

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

        var contactStep = HttpContext.Session.GetObject<ApplyContactViewModel>("apply_contact");
        if (contactStep == null) return RedirectToAction("Contact");

        // Required documents
        if (model.CertifiedId == null || model.CertifiedId.Length == 0)
            ModelState.AddModelError(string.Empty, "Please upload your Certified ID.");
        if (model.ProofOfIncome == null || model.ProofOfIncome.Length == 0)
            ModelState.AddModelError(string.Empty, "Please upload your Proof of income.");
        if (contactStep.IsDependant == "Yes" && (model.DependentId == null || model.DependentId.Length == 0))
            ModelState.AddModelError(string.Empty, "Please upload the Dependent ID.");

        // Size, extension and real file signature (PDF / JPG / PNG, maximum 5 MB)
        foreach (var (file, label) in new[] { (model.CertifiedId, "Certified ID"), (model.ProofOfIncome, "Proof of income"), (model.DependentId, "Dependent ID") })
        {
            var problem = await FileRules.CheckAsync(file, label);
            if (problem != null) ModelState.AddModelError(string.Empty, problem);
        }

        // Verification: the two main documents are required, and every file must be a real PDF / JPG / PNG
        if (model.CertifiedId == null || model.CertifiedId.Length == 0)
            ModelState.AddModelError(string.Empty, "Upload your certified ID copy.");
        if (model.ProofOfIncome == null || model.ProofOfIncome.Length == 0)
            ModelState.AddModelError(string.Empty, "Upload your proof of income.");
        foreach (var (file, label) in new[] { (model.CertifiedId, "Certified ID copy"), (model.ProofOfIncome, "Proof of income"), (model.DependentId, "Dependent ID") })
        {
            if (file != null && file.Length > 0)
            {
                var problem = await DocumentValidator.ValidateAsync(file, label);
                if (problem != null) ModelState.AddModelError(string.Empty, problem);
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

        // Do not carry on when a file could not be stored
        if (idUrl == "upload-failed" || incomeUrl == "upload-failed" || depUrl == "upload-failed")
        {
            ModelState.AddModelError(string.Empty, "A file could not be uploaded. Please try again.");
            return View(model);
        }

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

        var personalStep = HttpContext.Session.GetObject<ApplyPersonalViewModel>("apply_personal");
        if (personalStep != null && model.Password.Contains(personalStep.IdNumber))
        {
            ModelState.AddModelError("Password", "The password must not contain your ID Number.");
            return View(model);
        }

        HttpContext.Session.SetObject("apply_password", model);
        return RedirectToAction("Review");
    }

    // ═════════════════════════════════════════════
    // REVIEW
    // ═════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Review()
    {
        var review = BuildReviewModel();
        await FillSiteNameAsync(review);
        return View(review);
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
            var failedReview = BuildReviewModel();
            await FillSiteNameAsync(failedReview);
            return View("Review", failedReview);
        }

        // ---- Server-side re-check of everything collected in the earlier steps ----
        var reliable = HttpContext.Session.GetObject<ApplyReliableViewModel>("apply_reliable");
        var problems = new List<string>();
        if (reliable == null) problems.Add("Please complete the Reliable Information step.");
        foreach (object step in new object?[] { reliable, personal, contact, passwordStep })
        {
            if (step == null) continue;
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(step, new ValidationContext(step), results, true);
            problems.AddRange(results.Select(r => r.ErrorMessage ?? "Invalid information.").Distinct());
        }
        if (string.IsNullOrEmpty(idUrl) || idUrl == "upload-failed") problems.Add("The Certified ID document is missing.");
        if (string.IsNullOrEmpty(incomeUrl) || incomeUrl == "upload-failed") problems.Add("The Proof of income document is missing.");
        if (contact.IsDependant == "Yes" && (string.IsNullOrEmpty(depUrl) || depUrl == "upload-failed")) problems.Add("The Dependent ID document is missing.");
        if (!Guid.TryParse(contact.SelectedSiteId, out var siteGuid)) problems.Add("The selected site is not valid.");
        if (problems.Count == 0)
        {
            var site = await _siteService.GetSiteByIdAsync(siteGuid);
            if (site == null || !site.IsAvailable) problems.Add("The selected site is no longer available. Go back and choose another site.");
            if (await _applicationService.IdNumberExistsAsync(personal.IdNumber)) problems.Add("An application already exists for this ID Number.");
        }
        if (problems.Count > 0)
        {
            ViewData["ErrorMessage"] = string.Join(" ", problems);
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
            if (IsOfficer())
            {
                // A PTO Officer captured this application for a beneficiary: keep the officer signed in
                foreach (var key in new[] { "apply_reliable", "apply_personal", "apply_contact", "apply_dependent", "apply_password",
                                            "doc_certified_id_url", "doc_proof_income_url", "doc_dependent_id_url" })
                    HttpContext.Session.Remove(key);
            }
            else
            {
                HttpContext.Session.Clear();
            }
            HttpContext.Session.SetString("app_reference", personal.IdNumber);
            return RedirectToAction("Submitted");
        }
        catch (Exception ex)
        {
            ViewData["ErrorMessage"] = $"Database Error: {ex.Message}";
            var failedReview = BuildReviewModel();
            await FillSiteNameAsync(failedReview);
            return View("Review", failedReview);
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

    // Looks up the real site name for the Review screen (read-only)
    private async Task FillSiteNameAsync(ApplyReviewViewModel review)
    {
        var contact = HttpContext.Session.GetObject<ApplyContactViewModel>("apply_contact");
        if (contact != null && Guid.TryParse(contact.SelectedSiteId, out var siteId))
        {
            var site = await _siteService.GetSiteByIdAsync(siteId);
            if (site != null) review.SiteName = $"{site.SiteName} - {site.Location}";
        }
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