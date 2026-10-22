using System.ComponentModel.DataAnnotations;

namespace PTO_web_application.Validation;

/// <summary>
/// South African ID number rules (13 digits: YYMMDD, gender SSSS, citizenship C, A, check digit Z).
/// Pure helper: it does not touch the database.
/// </summary>
public static class SaId
{
    /// <summary>
    /// When true (default) the 13th digit must pass the ID check-digit test. Set the setting
    /// "Validation:EnforceIdCheckDigit" to false in appsettings.Development.json if you want to
    /// test with made-up 13-digit numbers.
    /// </summary>
    public static bool EnforceCheckDigit { get; set; } = true;

    public static bool IsWellFormed(string? id)
    {
        if (string.IsNullOrEmpty(id) || id.Length != 13) return false;
        foreach (var c in id) if (c < '0' || c > '9') return false;
        return true;
    }

    /// <summary>Luhn check digit (the 13th digit).</summary>
    public static bool PassesCheckDigit(string id)
    {
        if (!IsWellFormed(id)) return false;
        int sum = 0;
        for (int i = 0; i < 13; i++)
        {
            int digit = id[12 - i] - '0';
            if (i % 2 == 1)
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }
            sum += digit;
        }
        return sum % 10 == 0;
    }

    /// <summary>Date of birth from the first six digits. Same century rule as the Personal step (00-25 = 2000s).</summary>
    public static bool TryGetDateOfBirth(string id, out DateTime dob)
    {
        dob = default;
        if (!IsWellFormed(id)) return false;
        int yy = int.Parse(id.Substring(0, 2));
        int mm = int.Parse(id.Substring(2, 2));
        int dd = int.Parse(id.Substring(4, 2));
        int year = yy <= 25 ? 2000 + yy : 1900 + yy;
        if (mm < 1 || mm > 12) return false;
        if (dd < 1 || dd > DateTime.DaysInMonth(year, mm)) return false;
        dob = new DateTime(year, mm, dd);
        return true;
    }

    /// <summary>"Female" when digits 7-10 are below 5000, otherwise "Male".</summary>
    public static string GenderFromId(string id)
    {
        int seq = int.Parse(id.Substring(6, 4));
        return seq < 5000 ? "Female" : "Male";
    }

    /// <summary>0 = SA citizen, 1 = permanent resident.</summary>
    public static bool IsCitizen(string id) => id[10] == '0';

    public static int AgeOn(DateTime dob, DateTime today)
    {
        int age = today.Year - dob.Year;
        if (dob.Date > today.AddYears(-age)) age--;
        return age;
    }

    /// <summary>Returns an error message, or null when the ID number is fine.</summary>
    public static string? Check(string? id)
    {
        if (!IsWellFormed(id)) return "ID Number must be exactly 13 digits.";
        if (!TryGetDateOfBirth(id!, out var dob)) return "The first six digits of the ID Number are not a real date of birth.";
        if (dob.Date > DateTime.Today) return "The ID Number gives a date of birth in the future.";
        if (id![10] != '0' && id[10] != '1') return "The ID Number is not valid (citizenship digit).";
        if (EnforceCheckDigit && !PassesCheckDigit(id)) return "The ID Number is not valid (check digit does not match).";
        return null;
    }
}

/// <summary>Property-level check: [SaIdNumber] on an ID number property. Empty values are left to [Required].</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SaIdNumberAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var id = value as string;
        if (string.IsNullOrWhiteSpace(id)) return ValidationResult.Success;
        var problem = SaId.Check(id.Trim());
        return problem == null ? ValidationResult.Success : new ValidationResult(problem);
    }
}
