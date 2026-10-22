using System.ComponentModel.DataAnnotations;
using PTO_web_application.Validation;

namespace PTO_web_application.Models.ViewModels;

public class AddSiteViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Site number is required")]
    [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9 \-/]{0,19}$", ErrorMessage = "Site number: letters, digits, spaces, - or / (maximum 20 characters).")]
    public string SiteNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Settlement is required")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Settlement must be 2 to 60 characters.")]
    public string Settlement { get; set; } = string.Empty;

    [Required(ErrorMessage = "Site size is required")]
    public string SiteSize { get; set; } = string.Empty;

    [StringLength(60, ErrorMessage = "Location / GPS is too long.")]
    public string LocationGps { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Site size (m2): a number between 1 and 10 000
        if (!string.IsNullOrWhiteSpace(SiteSize))
        {
            var cleaned = SiteSize.Trim().Replace(",", ".");
            if (!decimal.TryParse(cleaned, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out var size) || size < 1 || size > 10000)
            {
                yield return new ValidationResult("Site size must be a number between 1 and 10 000 (square metres).", new[] { nameof(SiteSize) });
            }
        }

        // Location / GPS is optional; when given it must be "latitude, longitude" (for example -29.12, 26.21)
        if (!string.IsNullOrWhiteSpace(LocationGps))
        {
            var parts = LocationGps.Split(',', StringSplitOptions.TrimEntries);
            bool ok = parts.Length == 2
                && double.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat)
                && double.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lng)
                && lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180;
            if (!ok)
                yield return new ValidationResult("Location / GPS must look like: -29.1234, 26.2145", new[] { nameof(LocationGps) });
        }
    }
}