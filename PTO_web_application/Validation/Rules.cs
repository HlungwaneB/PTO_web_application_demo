using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PTO_web_application.Validation;

/// <summary>Phone, e-mail, name and text rules shared by the forms.</summary>
public static class Rules
{
    public const decimal MaxMonthlyIncome = 3500m;
    public const int MinAge = 21;
    public const long MaxFileBytes = 5 * 1024 * 1024;

    private static readonly Regex PhoneRegex = new(@"^0\d{9}$", RegexOptions.Compiled);
    private static readonly Regex NameRegex = new(@"^[\p{L}][\p{L} .'\-]*$", RegexOptions.Compiled);
    private static readonly Regex InitialsRegex = new(@"^[\p{L}]{1,10}$", RegexOptions.Compiled);

    /// <summary>Removes spaces, dashes and a +27 prefix so 082 123 4567 and +27821234567 are both accepted.</summary>
    public static string NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        var p = Regex.Replace(phone, @"[\s\-\(\)]", "");
        if (p.StartsWith("+27")) p = "0" + p.Substring(3);
        else if (p.StartsWith("0027")) p = "0" + p.Substring(4);
        return p;
    }

    public static bool IsPhone(string? phone) => PhoneRegex.IsMatch(NormalizePhone(phone));
    public static bool IsName(string? s) => !string.IsNullOrWhiteSpace(s) && s.Trim().Length <= 60 && NameRegex.IsMatch(s.Trim());
    public static bool IsInitials(string? s) => !string.IsNullOrWhiteSpace(s) && InitialsRegex.IsMatch(s.Trim().Replace(".", ""));

    public static bool IsEmail(string? s)
    {
        if (string.IsNullOrWhiteSpace(s) || s.Length > 254) return false;
        return new EmailAddressAttribute().IsValid(s.Trim());
    }

    /// <summary>At least 8 characters with an upper-case letter, a lower-case letter and a digit.</summary>
    public static string? CheckPassword(string? password)
    {
        if (string.IsNullOrEmpty(password)) return "Password is required.";
        if (password.Length < 8) return "Password must be at least 8 characters.";
        if (password.Length > 72) return "Password must be 72 characters or fewer.";
        if (!password.Any(char.IsUpper)) return "Password must contain an upper-case letter.";
        if (!password.Any(char.IsLower)) return "Password must contain a lower-case letter.";
        if (!password.Any(char.IsDigit)) return "Password must contain a digit.";
        return null;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class StrongPasswordAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var problem = Rules.CheckPassword(value as string);
        return problem == null ? ValidationResult.Success : new ValidationResult(problem);
    }
}

/// <summary>Optional or required South African cellphone number (10 digits starting with 0).</summary>
[AttributeUsage(AttributeTargets.Property)]
public class SaCellphoneAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var s = value as string;
        if (string.IsNullOrWhiteSpace(s)) return ValidationResult.Success; // use [Required] when mandatory
        return Rules.IsPhone(s) ? ValidationResult.Success
            : new ValidationResult("Enter a valid 10-digit South African cellphone number (for example 0821234567).");
    }
}

/// <summary>Letters, spaces, apostrophes, dots and hyphens only (names).</summary>
[AttributeUsage(AttributeTargets.Property)]
public class PersonNameAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var s = value as string;
        if (string.IsNullOrWhiteSpace(s)) return ValidationResult.Success;
        return Rules.IsName(s) ? ValidationResult.Success
            : new ValidationResult("Use letters only (spaces, hyphens and apostrophes are allowed), maximum 60 characters.");
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class InitialsAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var s = value as string;
        if (string.IsNullOrWhiteSpace(s)) return ValidationResult.Success;
        return Rules.IsInitials(s) ? ValidationResult.Success
            : new ValidationResult("Initials must be letters only (maximum 10).");
    }
}
