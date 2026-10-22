using System.ComponentModel.DataAnnotations;

namespace PTO_web_application.Models.ViewModels;

// 🟢 Notice the attribute is here
[IdNumberDobMatch]
public class ApplyPersonalViewModel
{
    [Required(ErrorMessage = "Surname is required")]
    public string Surname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Initials are required")]
    public string Initials { get; set; } = string.Empty;

    public string? Title { get; set; }

    [Required(ErrorMessage = "First name(s) are required")]
    public string FirstNames { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of Birth is required")]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "ID Number is required")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "ID Number must be exactly 13 digits.")]
    public string IdNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required")]
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