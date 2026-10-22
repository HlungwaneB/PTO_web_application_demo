using System.ComponentModel.DataAnnotations;

namespace PTO_web_application.Models.ViewModels;

public class ApplyReliableViewModel
{
    [Required(ErrorMessage = "Monthly income is required")]
    public string MonthlyIncome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select if you are South African")]
    public string IsSouthAfrican { get; set; } = "Yes";

    [Required(ErrorMessage = "Please select if you are a first time home owner")]
    public string IsFirstTimeHomeOwner { get; set; } = "Yes";

    [Required(ErrorMessage = "Please select if you are over 21 years old")]
    public string IsOver21 { get; set; } = "Yes";

    [Required(ErrorMessage = "Please select if this is your first time government subsidy")]
    public string IsFirstTimeGovernmentSubsidy { get; set; } = "Yes";

    /// <summary>
    /// Custom validation to ensure they are over 21 and earn less than 3500
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // 1. Check Age
        if (IsOver21 == "No")
        {
            yield return new ValidationResult(
                "You must be over 21 years old to apply.",
                new[] { nameof(IsOver21) });
        }

        // 2. Check Income
        if (decimal.TryParse(MonthlyIncome, out decimal income))
        {
            if (income >= 3500)
            {
                yield return new ValidationResult(
                    "Your monthly income must be less than R3500 to qualify.",
                    new[] { nameof(MonthlyIncome) });
            }
        }
        else
        {
            yield return new ValidationResult(
                "Please enter a valid numerical amount for income.",
                new[] { nameof(MonthlyIncome) });
        }
    }
}