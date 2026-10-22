using System.ComponentModel.DataAnnotations;
using PTO_web_application.Validation;

namespace PTO_web_application.Models.ViewModels;

// 🟢 Notice the attribute is here
[IdNumberDobMatch]
[SaIdApplicantRules]
public class ApplyPersonalViewModel
{
    [Required(ErrorMessage = "Surname is required")]
    [PersonName]
    public string Surname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Initials are required")]
    [Initials]
    public string Initials { get; set; } = string.Empty;

    [StringLength(10, ErrorMessage = "Title must be 10 characters or fewer.")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "First name(s) are required")]
    [PersonName]
    public string FirstNames { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of Birth is required")]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "ID Number is required")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "ID Number must be exactly 13 digits.")]
    [SaIdNumber]
    public string IdNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required")]
    [RegularExpression("^(Male|Female)$", ErrorMessage = "Select Male or Female.")]
    public string Gender { get; set; } = string.Empty;
}

// 🟢 The validation class is right here at the bottom, in the same namespace
public class IdNumberDobMatchAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance as ApplyPersonalViewModel;
        if (model == null) return ValidationResult.Success;

        if (string.IsNullOrEmpty(model.IdNumber) || !model.DateOfBirth.HasValue)
            return ValidationResult.Success;

        try
        {
            string yy = model.IdNumber.Substring(0, 2);
            string mm = model.IdNumber.Substring(2, 2);
            string dd = model.IdNumber.Substring(4, 2);

            int yearShort = int.Parse(yy);
            int yearFull = yearShort <= 25 ? 2000 + yearShort : 1900 + yearShort;

            var dobFromId = new DateTime(yearFull, int.Parse(mm), int.Parse(dd)).Date;

            if (model.DateOfBirth.Value.Date != dobFromId)
            {
                return new ValidationResult($"Date of Birth does not match the ID Number (Expected: {dobFromId:yyyy-MM-dd}).");
            }
        }
        catch
        {
            return new ValidationResult("Invalid ID Number or Date of Birth format.");
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Extra applicant rules that need several fields at once: the gender must match the ID number
/// and the applicant must be at least 21 (class-level, so it does not change the existing DOB check above).
/// </summary>
public class SaIdApplicantRulesAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var model = value as ApplyPersonalViewModel;
        if (model == null || !SaId.IsWellFormed(model.IdNumber)) return ValidationResult.Success;
        if (SaId.Check(model.IdNumber) != null) return ValidationResult.Success; // reported by [SaIdNumber]

        if (!string.IsNullOrEmpty(model.Gender) && SaId.GenderFromId(model.IdNumber) != model.Gender)
            return new ValidationResult($"The gender does not match the ID Number (the ID Number says {SaId.GenderFromId(model.IdNumber)}).");

        if (SaId.TryGetDateOfBirth(model.IdNumber, out var dob) && SaId.AgeOn(dob, DateTime.Today) < Rules.MinAge)
            return new ValidationResult($"You must be at least {Rules.MinAge} years old to apply.");

        if (!SaId.IsCitizen(model.IdNumber))
            return new ValidationResult("The ID Number is for a permanent resident. Only South African citizens can apply.");

        return ValidationResult.Success;
    }
}