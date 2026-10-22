namespace PTO_web_application.Models.ViewModels;

public class TrackResultViewModel
{
    public string IdNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? SubmittedOn { get; set; }
    public DateTime? ValidUntil { get; set; }

    // 🟢 Helper to color the status text in the View
    public string StatusClass => Status.ToLower() switch
    {
        "approved" => "status-approved",   // Green
        "rejected" => "status-rejected",   // Red
        "verifying" => "status-verifying", // Orange
        "pending" => "status-pending",     // Orange
        _ => "status-pending"
    };
}