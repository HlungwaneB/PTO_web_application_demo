namespace PTO_web_application.Models.ViewModels;

public class ApplyReviewViewModel
{
    // From Personal
    public string IdNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string FirstNames { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;

    // From Contact
    public string Cellphone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string HomeCellphone { get; set; } = string.Empty;
    public string IsDependant { get; set; } = string.Empty;

    // Site (will be set later — placeholder for now)
    public string SiteName { get; set; } = "Mangweni Z1";

    // Helper for display
    public string FullName => $"{Title} {Initials} {Surname}".Trim();
}