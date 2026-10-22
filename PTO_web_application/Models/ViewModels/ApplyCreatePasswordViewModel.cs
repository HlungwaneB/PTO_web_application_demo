using System.ComponentModel.DataAnnotations;
using PTO_web_application.Validation;

namespace PTO_web_application.Models.ViewModels;

public class ApplyCreatePasswordViewModel
{
    [Required(ErrorMessage = "Password is required")]
    [StrongPassword]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}