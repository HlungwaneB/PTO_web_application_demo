using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using PTO_web_application.Models.ViewModels;
using PTO_web_application.Services;


namespace PTO_web_application.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationService _applicationService;
    private readonly SiteService _siteService;
    private readonly Supabase.Client _supabase;

    public AdminController(
        ApplicationService applicationService,
        SiteService siteService,
        Supabase.Client supabase)
    {
        _applicationService = applicationService;
        _siteService = siteService;
        _supabase = supabase;
    }


    private bool IsAdmin()
    {
        var idNumber = HttpContext.Session.GetString("sb-id-number");
        if (string.IsNullOrEmpty(idNumber)) return false;

        // Only allow the "admin" ID number to access the admin area
        return idNumber.Trim().ToLower() == "admin";
    }








    // ═════════════════════════════════════════════
    // DASHBOARD (Live Stats)
    // ═════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        // 1. Fetch all applications and sites
        var allApps = await _applicationService.GetAllApplicationsAsync();
        var allSites = await _siteService.GetAvailableSitesAsync();

        // 2. Calculate the stats dynamically
        var model = new AdminDashboardViewModel
        {
            Beneficiaries = allApps.Select(a => a.IdNumber).Distinct().Count(),
            Sites = allSites.Count,
            ActivePtos = allApps.Count(a => a.CurrentStatus == "Approved"),
            PendingApplications = allApps.Count(a => a.CurrentStatus == "Pending" || a.CurrentStatus == "Verifying"),
            DeRegistered = allApps.Count(a => a.CurrentStatus == "Cancelled" || a.CurrentStatus == "Rejected"),

            // Expiry logic
            Expired = allApps.Count(a => a.CurrentStatus == "Approved" && a.EndDate < DateTime.UtcNow),
            ApproachingExpiry = allApps.Count(a => a.CurrentStatus == "Approved" && a.EndDate >= DateTime.UtcNow && a.EndDate <= DateTime.UtcNow.AddMonths(2)),

            // Placeholder for Renewed
            Renewed = 0,

            // 3. Get the top 3 "Pending" applications for the Needs Attention table
            NeedsAttention = allApps
                .Where(a => a.CurrentStatus == "Pending" || a.CurrentStatus == "Verifying")
                .Take(3)
                .Select(a => new AttentionItem
                {
                    Applicant = a.FullName,
                    Site = allSites.FirstOrDefault(s => s.SiteId == a.SiteId)?.SiteName ?? "Unknown",
                    Settlement = allSites.FirstOrDefault(s => s.SiteId == a.SiteId)?.Location ?? "Unknown",
                    Submitted = a.StartDate.ToString("dd MMM yyyy")
                }).ToList()
        };

        return View(model);
    }

    // ═════════════════════════════════════════════
    // APPLICATIONS (We will wire this up next)
    // ═════════════════════════════════════════════
    // ═════════════════════════════════════════════
    // APPLICATIONS
    // ═════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Applications(string search = "", string filter = "All")
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        // Fetch all applications and sites from the database
        var allApps = await _applicationService.GetAllApplicationsAsync();
        var allSites = await _siteService.GetAvailableSitesAsync();

        // Apply Filter (All, Pending, Approved, Rejected)
        if (filter != "All")
        {
            if (filter == "Pending")
                allApps = allApps.Where(a => a.CurrentStatus == "Pending" || a.CurrentStatus == "Verifying").ToList();
            else
                allApps = allApps.Where(a => a.CurrentStatus.Equals(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Apply Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            allApps = allApps.Where(a =>
                a.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                a.IdNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        // Map to ViewModel
        var model = new AdminApplicationsViewModel
        {
            SearchQuery = search,
            ActiveFilter = filter,
            Applications = allApps.Select(a => new ApplicationRow
            {
                ApplicantName = a.FullName,
                IdNumber = a.IdNumber,
                Site = allSites.FirstOrDefault(s => s.SiteId == a.SiteId)?.SiteName ?? "Unknown Site",
                Settlement = allSites.FirstOrDefault(s => s.SiteId == a.SiteId)?.Location ?? "Unknown",
                SubmittedDate = a.StartDate.ToString("dd MMM yyyy"),
                Status = a.CurrentStatus
            }).ToList()
        };

        return View(model);
    }

    // ═════════════════════════════════════════════
    // PTO RECORDS (We will wire this up next)
    // ═════════════════════════════════════════════
    // ═════════════════════════════════════════════
    // PTO RECORDS
    // ═════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> PtoRecords(string search = "", string filter = "All")
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var allApps = await _applicationService.GetAllApplicationsAsync();
        var allSites = await _siteService.GetAvailableSitesAsync();

        // Only show Approved or Verifying in the PTO Records
        var records = allApps.Where(a => a.CurrentStatus == "Approved" || a.CurrentStatus == "Verifying").ToList();

        // Apply Filter
        if (filter != "All")
        {
            records = records.Where(a => a.CurrentStatus.Equals(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Apply Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            records = records.Where(a =>
                a.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                a.IdNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        // Map to ViewModel
        var model = new AdminPtoRecordsViewModel
        {
            SearchQuery = search,
            ActiveFilter = filter,
            Records = records.Select(a => new PtoRecordRow
            {
                PtoRef = $"PTO-{a.StartDate.Year}-{a.IdNumber.Substring(0, 5)}",
                Beneficiary = a.FullName,
                Site = allSites.FirstOrDefault(s => s.SiteId == a.SiteId)?.SiteName ?? "Unknown",
                ExpiryDate = a.EndDate.ToString("dd MMM yyyy"),
                Status = a.CurrentStatus
            }).ToList()
        };

        return View(model);
    }

    // ═════════════════════════════════════════════
    // SITES (We will wire this up next)
    // ═════════════════════════════════════════════
    // ═════════════════════════════════════════════
    // MANAGE SITES
    // ═════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Sites(string search = "", string filter = "All")
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        // Fetch all sites from the database
        var allSites = await _siteService.GetAvailableSitesAsync();

        // Apply Filter
        if (filter != "All")
        {
            if (filter == "Available")
                allSites = allSites.Where(s => s.IsAvailable).ToList();
            else if (filter == "Occupied")
                allSites = allSites.Where(s => !s.IsAvailable).ToList();
        }

        // Apply Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            allSites = allSites.Where(s =>
                s.SiteName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                s.Location.Contains(search, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        // Map to ViewModel
        var model = new AdminSitesViewModel
        {
            SearchQuery = search,
            ActiveFilter = filter,
            Sites = allSites.Select(s => new SiteRow
            {
                SiteId = s.SiteId, // 🟢 ADD THIS
                SiteNo = s.SiteName,
                Settlement = s.Location,
                Size = s.SizeDetails ?? "N/A",
                // If it's available, show "Available", otherwise "Active/Occupied"
                Status = s.IsAvailable ? "Available" : "Active"
            }).ToList()
        };

        return View(model);
    }

    // ═════════════════════════════════════════════
    // ADD SITE
    // ═════════════════════════════════════════════
    [HttpGet]
    public IActionResult AddSite()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");
        return View(new AddSiteViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSite(AddSiteViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        // 🟢 1. If form validation fails, show the errors on the screen
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var newSite = new Models.Site
            {
                SiteName = model.SiteNumber,
                Location = model.Settlement,
                SizeDetails = model.SiteSize,
                FeeAmount = 1500.00m,
                IsAvailable = true
            };

            // 🟢 2. Try saving to Supabase
            await _siteService.AddSiteAsync(newSite);

            return RedirectToAction("Sites");
        }
        catch (Exception ex)
        {
            // 🟢 3. If Supabase fails (e.g., RLS policy blocks it), show the error at the top of the form
            ModelState.AddModelError("", $"Supabase Error: {ex.Message}");
            return View(model);
        }
    }
    // ═════════════════════════════════════════════
    // REPORTS
    // ═════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Reports()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        // 1. Get current month and year for the report period
        var currentMonth = DateTime.UtcNow.ToString("MMM yyyy");
        var today = DateTime.UtcNow.ToString("dd MMM yyyy");

        // 2. Count some real stats from the database to show the system is working
        var allApps = await _applicationService.GetAllApplicationsAsync();
        var totalApps = allApps.Count;
        var approvedApps = allApps.Count(a => a.CurrentStatus == "Approved");

        // 3. Create the report list
        var model = new AdminReportsViewModel
        {
            Reports = new List<ReportRow>
            {
                new ReportRow
                {
                    ReportName = "PTO status summary",
                    Period = currentMonth,
                    GeneratedDate = today
                },
                new ReportRow
                {
                    ReportName = "Expiring PTOs (30 days)",
                    Period = currentMonth,
                    GeneratedDate = today
                },
                new ReportRow
                {
                    ReportName = "Sites by settlement",
                    Period = $"Q{Math.Ceiling(DateTime.UtcNow.Month / 3.0)} {DateTime.UtcNow.Year}",
                    GeneratedDate = today
                }
            }
        };

        return View(model);
    }





    // ═════════════════════════════════════════════
    // GENERATE REPORT (Top Button)
    // ═════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GenerateReport()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        // In a real app, this would run a heavy SQL query and save a file to storage.
        // For now, we will just simulate the process and refresh the page.

        // (We can add a success message here later using TempData)

        return RedirectToAction("Reports");
    }

    // ═════════════════════════════════════════════
    // EXPORT REPORT (PDF / Excel Buttons)
    // ═════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> ExportReport(string format)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var allApps = await _applicationService.GetAllApplicationsAsync();
        var fileName = $"PTO_Report_{DateTime.Now:yyyyMMdd_HHmm}";

        if (format.ToLower() == "excel")
        {
            // Generate CSV
            var csvBuilder = new System.Text.StringBuilder();
            csvBuilder.AppendLine("ID Number,Full Name,Email,Phone,Status,Start Date,End Date");
            foreach (var app in allApps)
            {
                csvBuilder.AppendLine($"{app.IdNumber},{app.FullName},{app.Email},{app.PhoneNumber},{app.CurrentStatus},{app.StartDate:yyyy-MM-dd},{app.EndDate:yyyy-MM-dd}");
            }
            var fileBytes = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(fileBytes, "text/csv", $"{fileName}.csv");
        }
        else if (format.ToLower() == "pdf")
        {
            // 🟢 Generate PDF using QuestPDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    // ✅ CORRECT (Calling Header once, adding two rows)
                    page.Header().Column(header =>
                    {
                        header.Item().Text("PTO Applications Report").SemiBold().FontSize(20).FontColor(Colors.Black);
                        header.Item().Text($"Generated on: {DateTime.Now:dd MMM yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Medium);
                    });
                    // Content Table
                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        // Define Columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(100); // Name
                            columns.ConstantColumn(100); // ID Number
                            columns.ConstantColumn(80);  // Status
                            columns.RelativeColumn();    // Start Date
                            columns.RelativeColumn();    // End Date
                        });

                        // Header Row
                        table.Header(header =>
                        {
                            header.Cell().Text("Applicant").Bold();
                            header.Cell().Text("ID Number").Bold();
                            header.Cell().Text("Status").Bold();
                            header.Cell().Text("Start Date").Bold();
                            header.Cell().Text("End Date").Bold();
                        });

                        // Data Rows
                        foreach (var app in allApps)
                        {
                            table.Cell().Text(app.FullName);
                            table.Cell().Text(app.IdNumber);
                            table.Cell().Text(app.CurrentStatus);
                            table.Cell().Text(app.StartDate.ToString("dd MMM yyyy"));
                            table.Cell().Text(app.EndDate.ToString("dd MMM yyyy"));
                        }
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            // Generate the PDF byte array
            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"{fileName}.pdf");
        }

        return RedirectToAction("Reports");
    }


    // ═════════════════════════════════════════════
    // AUDIT LOG (We will wire this up next)
    // ═════════════════════════════════════════════
    // ═════════════════════════════════════════════
    // AUDIT LOG
    // ═════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> AuditLog()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        // Fetch real data to generate the audit log
        var allApps = await _applicationService.GetAllApplicationsAsync();
        var allSites = await _siteService.GetAvailableSitesAsync();

        var logs = new List<AuditLogRow>();

        // 1. Create log entries for Applications
        foreach (var app in allApps.OrderByDescending(a => a.StartDate).Take(5))
        {
            logs.Add(new AuditLogRow
            {
                When = app.StartDate.ToString("dd MMM yyyy HH:mm"),
                User = app.FullName, // The applicant
                Action = "Submitted PTO application",
                Record = $"Ref: {app.IdNumber}"
            });
        }

        // 2. Create log entries for Sites
        foreach (var site in allSites.OrderByDescending(s => s.SiteId).Take(2)) // Just taking 2 for the demo
        {
            logs.Add(new AuditLogRow
            {
                When = DateTime.UtcNow.AddDays(-1).ToString("dd MMM yyyy HH:mm"), // Faking a date for the site
                User = "Administrator",
                Action = "Added new site",
                Record = site.SiteName
            });
        }

        // Sort the combined logs by date descending (newest first)
        logs = logs.OrderByDescending(l => DateTime.Parse(l.When)).ToList();

        var model = new AdminAuditLogViewModel
        {
            Logs = logs
        };

        return View(model);
    }

    // ═════════════════════════════════════════════
    // DELETE SITE
    // ═════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSite(Guid siteId)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        try
        {
            await _siteService.DeleteSiteAsync(siteId);
        }
        catch (Exception ex)
        {
            // 🟢 Throw the error so the yellow screen of death shows us exactly why it failed
            throw new Exception($"Failed to delete site: {ex.Message}");
        }

        return RedirectToAction("Sites");
    }
}