using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PTO_web_application.Models.ViewModels;

public class ApplyContactViewModel
{
    [Required(ErrorMessage = "Cellphone is required")]
    public string Cellphone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm cellphone is required")]
    public string ConfirmCellphone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string EmailAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm email address is required")]
    public string ConfirmEmailAddress { get; set; } = string.Empty;

    public string HomeCellphone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select if you are a dependant")]
    public string IsDependant { get; set; } = "No";

    // 🟢 Added for Site Selection
    [Required(ErrorMessage = "Please select a site")]
    public string SelectedSiteId { get; set; } = string.Empty;

    // 🟢 Holds the list of sites for the dropdown
    public List<SelectListItem> AvailableSites { get; set; } = new();
}