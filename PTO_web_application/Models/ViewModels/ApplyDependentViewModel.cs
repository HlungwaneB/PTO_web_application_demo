using System.ComponentModel.DataAnnotations;
using PTO_web_application.Validation;

namespace PTO_web_application.Models.ViewModels;

public class ApplyDependentViewModel
{
    [Required(ErrorMessage = "Surname is required")]
    [PersonName]
    public string Surname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Initials are required")]
    [Initials]
    public string Initials { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name(s) are required")]
    [PersonName]
    public string FirstNames { get; set; } = string.Empty;

    [Required(ErrorMessage = "ID Number is required")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "ID Number must be exactly 13 digits.")]
    [SaIdNumber]
    public string IdNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cellphone is required")]
    [SaCellphone]
    public string Cellphone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(254, ErrorMessage = "Email address is too long.")]
    public string EmailAddress { get; set; } = string.Empty;
}