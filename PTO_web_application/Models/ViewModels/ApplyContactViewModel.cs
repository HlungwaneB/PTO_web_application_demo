using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using PTO_web_application.Validation;

namespace PTO_web_application.Models.ViewModels;

public class ApplyContactViewModel
{
    [Required(ErrorMessage = "Cellphone is required")]
    [SaCellphone]
    public string Cellphone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm cellphone is required")]
    [SaCellphone]
    public string ConfirmCellphone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(254, ErrorMessage = "Email address is too long.")]
    public string EmailAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm email address is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string ConfirmEmailAddress { get; set; } = string.Empty;

    [SaCellphone]
    public string HomeCellphone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select if you are a dependant")]
    [RegularExpression("^(Yes|No)$", ErrorMessage = "Answer YES or NO.")]
    public string IsDependant { get; set; } = "No";

    // 🟢 Added for Site Selection
    [Required(ErrorMessage = "Please select a site")]
    public string SelectedSiteId { get; set; } = string.Empty;

    // 🟢 Holds the list of sites for the dropdown
    public List<SelectListItem> AvailableSites { get; set; } = new();
}