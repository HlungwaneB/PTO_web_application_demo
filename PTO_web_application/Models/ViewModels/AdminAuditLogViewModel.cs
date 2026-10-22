namespace PTO_web_application.Models.ViewModels;

public class AdminAuditLogViewModel
{
    public List<AuditLogRow> Logs { get; set; } = new();
}

public class AuditLogRow
{
    public string When { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Record { get; set; } = string.Empty;
}