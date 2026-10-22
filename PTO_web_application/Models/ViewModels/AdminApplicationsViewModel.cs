namespace PTO_web_application.Models.ViewModels;

public class AdminApplicationsViewModel
{
    public string SearchQuery { get; set; } = string.Empty;
    public string ActiveFilter { get; set; } = "All"; // All, Pending, Approved, Rejected
    public List<ApplicationRow> Applications { get; set; } = new();
}

public class ApplicationRow
{
    public string ApplicantName { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string Site { get; set; } = string.Empty;
    public string Settlement { get; set; } = string.Empty;
    public string SubmittedDate { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";

    // Helper for CSS classes
    public string StatusClass => Status.ToLower() switch
    {
        "approved" => "badge-approved",
        "rejected" => "badge-rejected",
        "verifying" => "badge-pending",
        "pending" => "badge-pending",
        _ => "badge-pending"
    };
}