using System.ComponentModel.DataAnnotations;

namespace PTO_web_application.Validation;

/// <summary>
/// Verifies a South African ID number (13 digits): YYMMDD date of birth, SSSS gender sequence,
/// C citizenship digit (0 = citizen, 1 = permanent resident) and Z, the Luhn check digit.
/// A made-up ID number is rejected. (There is no Home Affairs lookup: the app checks the number itself.)
/// Pure code - it does not touch the database.
/// </summary>
public static class SaIdValidator
{
    public static bool TryParse(string? id, out DateTime dateOfBirth, out string gender, out bool isCitizen)
    {
        dateOfBirth = default;
        gender = string.Empty;
        isCitizen = false;

        if (string.IsNullOrWhiteSpace(id) || id.Length != 13 || !id.All(char.IsDigit))
            return false;

        int yy = int.Parse(id.Substring(0, 2));
        int mm = int.Parse(id.Substring(2, 2));
        int dd = int.Parse(id.Substring(4, 2));

        // Two-digit year: later than this year's last two digits means 19xx
        int currentYY = DateTime.UtcNow.Year % 100;
        int year = yy > currentYY ? 1900 + yy : 2000 + yy;

        try { dateOfBirth = new DateTime(year, mm, dd); }
        catch (ArgumentOutOfRangeException) { return false; }

        if (dateOfBirth.Date > DateTime.UtcNow.Date) return false;

        int sequence = int.Parse(id.Substring(6, 4));
        gender = sequence >= 5000 ? "Male" : "Female";
        isCitizen = id[10] == '0';

        return PassesLuhn(id);
    }

    public static bool IsValid(string? id) => TryParse(id, out _, out _, out _);

    /// <summary>Luhn checksum: double every second digit from the right (the check digit is not doubled).</summary>
    private static bool PassesLuhn(string id)
    {
        int sum = 0;
        bool doubleIt = false;
        for (int i = id.Length - 1; i >= 0; i--)
        {
            int d = id[i] - '0';
            if (doubleIt)
            {
                d *= 2;
                if (d > 9) d -= 9;
            }
            sum += d;
            doubleIt = !doubleIt;
        }
        return sum % 10 == 0;
    }

    public static int AgeOn(DateTime dateOfBirth, DateTime today)
    {
        int age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}

/// <summary>[SouthAfricanId] - put on a string property to validate the 13-digit ID number.</summary>
public class SouthAfricanIdAttribute : ValidationAttribute
{
    public SouthAfricanIdAttribute()
    {
        ErrorMessage = "Enter a valid 13-digit South African ID number.";
    }

    public override bool IsValid(object? value)
    {
        var text = value as string;
        if (string.IsNullOrEmpty(text)) return true; // [Required] handles empty values
        return SaIdValidator.IsValid(text);
    }
}

/// <summary>
/// Verifies an uploaded supporting document: size, extension AND the real file signature
/// (so a .exe renamed to .pdf is rejected). Pure code - the upload itself stays in ApplyController.
/// </summary>
public static class DocumentValidator
{
    private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
    private const long MaxBytes = 5 * 1024 * 1024;

    /// <summary>Returns an error message, or null when the file is acceptable.</summary>
    public static async Task<string?> ValidateAsync(IFormFile file, string label)
    {
        if (file.Length > MaxBytes)
            return $"{label}: the file is too large. The maximum size is 5 MB.";

        var extension = Path.GetExtension(file.FileName ?? "").ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return $"{label}: only PDF, JPG or PNG files are allowed.";

        var header = new byte[8];
        using (var stream = file.OpenReadStream())
        {
            int read = await stream.ReadAsync(header, 0, header.Length);
            if (read < 4) return $"{label}: the file is empty or damaged.";
        }

        bool isPdf = header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46;   // %PDF
        bool isPng = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;   // .PNG
        bool isJpg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;                        // JPEG

        bool matches = (extension == ".pdf" && isPdf) || (extension == ".png" && isPng) ||
                       ((extension == ".jpg" || extension == ".jpeg") && isJpg);

        return matches ? null : $"{label}: the file content does not match its type. Upload a real PDF, JPG or PNG.";
    }
}
