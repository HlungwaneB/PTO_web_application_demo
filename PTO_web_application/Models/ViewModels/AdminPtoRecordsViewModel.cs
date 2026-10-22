namespace PTO_web_application.Models.ViewModels;

public class AdminPtoRecordsViewModel
{
    public string SearchQuery { get; set; } = string.Empty;
    public string ActiveFilter { get; set; } = "All";
    public List<PtoRecordRow> Records { get; set; } = new();
}

public class PtoRecordRow
{
    public string PtoRef { get; set; } = string.Empty;
    public string Beneficiary { get; set; } = string.Empty;
    public string Site { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public string StatusClass => Status.ToLower() switch
    {
        "active" => "badge-active",
        "verifying" => "badge-expiring",
        "approved" => "badge-active",
        "expired" => "badge-expired",
        _ => "badge-pending"
    };
}