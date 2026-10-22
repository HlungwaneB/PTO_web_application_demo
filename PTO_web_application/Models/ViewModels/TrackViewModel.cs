using System.ComponentModel.DataAnnotations;

namespace PTO_web_application.Models.ViewModels;

public class TrackViewModel
{
    [Required(ErrorMessage = "Please enter an ID number")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "ID number must be exactly 13 digits.")]
    public string IdNumber { get; set; } = string.Empty;
}