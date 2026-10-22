namespace PTO_web_application.Validation;

/// <summary>Checks an uploaded document: size, extension and the real file signature (not only the name).</summary>
public static class FileRules
{
    private static readonly string[] Allowed = { ".pdf", ".jpg", ".jpeg", ".png" };

    /// <summary>Returns an error message, or null when the file is acceptable.</summary>
    public static async Task<string?> CheckAsync(IFormFile? file, string label)
    {
        if (file == null || file.Length == 0) return null; // "required" is decided by the caller

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext)) return $"{label}: only PDF, JPG or PNG files are allowed.";
        if (file.Length > Rules.MaxFileBytes) return $"{label}: the file is larger than 5 MB.";

        var header = new byte[8];
        int read;
        using (var stream = file.OpenReadStream())
        {
            read = await stream.ReadAsync(header, 0, header.Length);
        }
        if (read < 4) return $"{label}: the file is empty or damaged.";

        bool pdf = header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46;                 // %PDF
        bool png = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;                 // PNG
        bool jpg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;                                      // JPEG

        bool matches = ext == ".pdf" ? pdf : ext == ".png" ? png : jpg;
        return matches ? null : $"{label}: the file content does not match its type ({ext}).";
    }
}
