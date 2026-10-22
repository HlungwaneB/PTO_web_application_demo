using PTO_web_application.Models;

namespace PTO_web_application.Models.ViewModels;

public class DashboardViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;

    public Application? CurrentApplication { get; set; }
    public Site? CurrentSite { get; set; }

    public bool HasApplication => CurrentApplication != null;
    public string SiteName => CurrentSite?.SiteName ?? "Not assigned";
    public string LeasePeriod =>
        CurrentApplication != null
            ? $"{CurrentApplication.StartDate:dd MMMM yyyy} - {CurrentApplication.EndDate:dd MMMM yyyy}"
            : "N/A";
    public string StatusBadgeClass => CurrentApplication?.CurrentStatus switch
    {
        "Pending" => "badge-pending",
        "Approved" => "badge-approved",
        "Rejected" => "badge-rejected",
        "Cancelled" => "badge-cancelled",
        "Completed" => "badge-completed",
        _ => "badge-pending"
    };
}