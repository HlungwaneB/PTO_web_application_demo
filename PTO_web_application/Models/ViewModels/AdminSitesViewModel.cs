namespace PTO_web_application.Models.ViewModels;

public class AdminSitesViewModel
{
    public string SearchQuery { get; set; } = string.Empty;
    public string ActiveFilter { get; set; } = "All";
    public List<SiteRow> Sites { get; set; } = new();
}

public class SiteRow
{
    public Guid SiteId { get; set; } // 🟢 ADD THIS
    public string SiteNo { get; set; } = string.Empty;
    public string Settlement { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Status { get; set; } = "Available";

    public string StatusClass => Status.ToLower() switch
    {
        "active" => "badge-active",       // Green
        "available" => "badge-active",    // Green
        "occupied" => "badge-expiring",   // Orange
        _ => "badge-pending"
    };
}