using Microsoft.AspNetCore.Mvc.Rendering;

namespace PTO_web_application.Models.ViewModels;

public class ProfileViewModel
{
    // 🟢 Tracks if the form is in edit mode
    public bool IsEditing { get; set; } = false;

    // Header Data
    public string FullName { get; set; } = "User";
    public string Initials { get; set; } = "U";

    // Tab State
    public string ActiveTab { get; set; } = "personal";

    // Personal Details
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;

    // Contact Details
    public string PrimaryMobile { get; set; } = string.Empty;
    public string PrimaryEmail { get; set; } = string.Empty;

    // Address Details
    public string StreetNumber { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public string PostalAddress { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "South Africa";
    public string AddressType { get; set; } = string.Empty;

    // Next of Kin
    public string KinName { get; set; } = string.Empty;
    public string KinSurname { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string KinPhone { get; set; } = string.Empty;
    public string KinEmail { get; set; } = string.Empty;
    public string KinAddress { get; set; } = string.Empty;

    // Dropdowns
    public List<SelectListItem> GenderOptions { get; set; } = new()
    {
        new SelectListItem { Value = "Male", Text = "Male" },
        new SelectListItem { Value = "Female", Text = "Female" }
    };
}