using PTO_web_application.Models;
using Supabase;
using static Supabase.Postgrest.Constants;

namespace PTO_web_application.Services;

public class ApplicationService
{
    private readonly Supabase.Client _supabase;

    public ApplicationService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    /// <summary>
    /// Submit a new application. Runs before login.
    /// </summary>
    public async Task<Application?> SubmitAsync(Application application)
    {
        try
        {
            var response = await _supabase
                .From<Application>()
                .Insert(application);

            return response.Models.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Submit failed: {ex.Message}");
            throw; // Let the controller catch the duplicate rule error
        }
    }

    /// <summary>
    /// Get the current user's application by ID number.
    /// Requires an authenticated session.
    /// </summary>
    public async Task<Application?> GetMyApplicationAsync(string idNumber)
    {
        try
        {
            var response = await _supabase
                .From<Application>()
                .Where(a => a.IdNumber == idNumber)
                .Order(a => a.ApplicationId, Ordering.Descending)
                .Limit(1)
                .Get();

            return response.Models.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetMyApplication failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Fetches ALL applications from the database. Used by the Admin Dashboard.
    /// </summary>
    public async Task<List<Application>> GetAllApplicationsAsync()
    {
        try
        {
            // We order by StartDate descending so the newest applications show first
            var response = await _supabase
                .From<Application>()
                .Order(a => a.StartDate, Ordering.Descending)
                .Get();

            return response.Models;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching all applications: {ex.Message}");
            return new List<Application>();
        }
    }

    // ---------------------------------------------------------------
    // Added helpers (read-only checks + one contact update). They use
    // only columns that already exist in the "applications" table.
    // ---------------------------------------------------------------

    /// <summary>True when an application already exists for this ID number.</summary>
    public async Task<bool> IdNumberExistsAsync(string idNumber)
    {
        try
        {
            var response = await _supabase
                .From<Application>()
                .Where(a => a.IdNumber == idNumber)
                .Limit(1)
                .Get();
            return response.Models.Any();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"IdNumberExists failed: {ex.Message}");
            return false; // do not block the applicant because of a read error; the database rule still protects us
        }
    }

    /// <summary>How many applications use this site (a site with applications must not be deleted).</summary>
    public async Task<int> CountBySiteAsync(Guid siteId)
    {
        var response = await _supabase
            .From<Application>()
            .Where(a => a.SiteId == siteId)
            .Get();
        return response.Models.Count;
    }

    /// <summary>Updates only the phone number and e-mail address of one applicant.</summary>
    public async Task UpdateContactAsync(string idNumber, string phoneNumber, string email)
    {
        await _supabase
            .From<Application>()
            .Where(a => a.IdNumber == idNumber)
            .Set(a => a.PhoneNumber, phoneNumber)
            .Set(a => a.Email, email)
            .Update();
    }
}
