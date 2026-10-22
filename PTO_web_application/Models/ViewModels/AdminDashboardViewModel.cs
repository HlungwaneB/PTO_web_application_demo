namespace PTO_web_application.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int Beneficiaries { get; set; }
    public int Sites { get; set; }
    public int ActivePtos { get; set; }
    public int ApproachingExpiry { get; set; }
    public int Expired { get; set; }
    public int Renewed { get; set; }
    public int DeRegistered { get; set; }
    public int PendingApplications { get; set; }

    public List<AttentionItem> NeedsAttention { get; set; } = new();
}

public class AttentionItem
{
    public string Applicant { get; set; } = string.Empty;
    public string Site { get; set; } = string.Empty;
    public string Settlement { get; set; } = string.Empty;
    public string Submitted { get; set; } = string.Empty;
}