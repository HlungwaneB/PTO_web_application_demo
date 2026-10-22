namespace PTO_web_application.Models.ViewModels;

public class AdminReportsViewModel
{
    public List<ReportRow> Reports { get; set; } = new();
}

public class ReportRow
{
    public string ReportName { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string GeneratedDate { get; set; } = string.Empty;
}