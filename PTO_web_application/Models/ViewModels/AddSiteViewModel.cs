using System.ComponentModel.DataAnnotations;

namespace PTO_web_application.Models.ViewModels;

public class AddSiteViewModel
{
    [Required(ErrorMessage = "Site number is required")]
    public string SiteNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Settlement is required")]
    public string Settlement { get; set; } = string.Empty;

    [Required(ErrorMessage = "Site size is required")]
    public string SiteSize { get; set; } = string.Empty;

    public string LocationGps { get; set; } = string.Empty;
}