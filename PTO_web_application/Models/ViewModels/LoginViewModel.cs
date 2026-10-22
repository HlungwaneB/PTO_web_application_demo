using System.ComponentModel.DataAnnotations;

namespace PTO_web_application.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "ID Number is required")]
    [Display(Name = "ID Number")]
    [RegularExpression(@"^(\d{13}|[Aa][Dd][Mm][Ii][Nn]|[Oo][Ff][Ff][Ii][Cc][Ee][Rr])$", ErrorMessage = "Enter your 13-digit ID number.")]
    public string IdNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [StringLength(72, ErrorMessage = "Password is too long.")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}