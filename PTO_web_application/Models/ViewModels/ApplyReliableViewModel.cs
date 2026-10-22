using System.ComponentModel.DataAnnotations;
using PTO_web_application.Validation;

namespace PTO_web_application.Models.ViewModels;

public class ApplyReliableViewModel : IValidatableObject
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
    /// Eligibility rules (runs automatically because the class implements IValidatableObject):
    /// income below R3 500, South African, first-time owner, over 21, first-time subsidy.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Income must be a real, non-negative number below the limit
        var cleaned = (MonthlyIncome ?? string.Empty).Replace(" ", "").Replace("R", "").Replace("r", "").Replace(",", ".");
        if (!decimal.TryParse(cleaned, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out decimal income))
        {
            yield return new ValidationResult("Please enter a valid numerical amount for income.", new[] { nameof(MonthlyIncome) });
        }
        else if (income < 0)
        {
            yield return new ValidationResult("Monthly income cannot be negative.", new[] { nameof(MonthlyIncome) });
        }
        else if (income >= Rules.MaxMonthlyIncome)
        {
            yield return new ValidationResult("Your monthly income must be less than R3500 to qualify.", new[] { nameof(MonthlyIncome) });
        }

        // The YES / NO answers are model-level errors so they show in the summary at the top of the form
        if (IsSouthAfrican != "Yes" && IsSouthAfrican != "No") yield return new ValidationResult("Answer YES or NO: are you South African?");
        else if (IsSouthAfrican == "No") yield return new ValidationResult("You must be a South African citizen to apply.");

        if (IsFirstTimeHomeOwner != "Yes" && IsFirstTimeHomeOwner != "No") yield return new ValidationResult("Answer YES or NO: are you a first time home owner?");
        else if (IsFirstTimeHomeOwner == "No") yield return new ValidationResult("You must be a first time home owner to apply.");

        if (IsOver21 != "Yes" && IsOver21 != "No") yield return new ValidationResult("Answer YES or NO: are you over 21 years old?");
        else if (IsOver21 == "No") yield return new ValidationResult("You must be over 21 years old to apply.");

        if (IsFirstTimeGovernmentSubsidy != "Yes" && IsFirstTimeGovernmentSubsidy != "No") yield return new ValidationResult("Answer YES or NO: is this your first time government subsidy?");
        else if (IsFirstTimeGovernmentSubsidy == "No") yield return new ValidationResult("You must be a first time government subsidy recipient to apply.");
    }
}