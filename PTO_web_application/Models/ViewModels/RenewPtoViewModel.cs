using System.ComponentModel.DataAnnotations;

namespace PTO_web_application.Models.ViewModels;

public class RenewPtoViewModel
{
    public DateTime? ExpiryDate { get; set; }

    // 🟢 Helper property to calculate days left
    public int DaysLeft => ExpiryDate.HasValue
        ? (ExpiryDate.Value - DateTime.Today).Days
        : 0;

    // 🟢 Helper property to format the text for the screen
    public string ExpiryText => ExpiryDate.HasValue
        ? $"{ExpiryDate.Value:dd MMM yyyy} — {DaysLeft} days left"
        : "No active PTO found";

    [RegularExpression("^(Yes|No)$", ErrorMessage = "Answer YES or NO.")]
    public string StillLivingAtSite { get; set; } = "Yes";
    [RegularExpression("^(Yes|No)$", ErrorMessage = "Answer YES or NO.")]
    public string ChangeInIncome { get; set; } = "No";
    [StringLength(500, ErrorMessage = "Notes can be at most 500 characters.")]
    public string Notes { get; set; } = string.Empty;
}