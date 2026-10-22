using System.ComponentModel.DataAnnotations;

namespace PTO_web_application.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "ID Number is required")]
    [Display(Name = "ID Number")]
    public string IdNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}