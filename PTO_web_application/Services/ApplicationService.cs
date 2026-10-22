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
}