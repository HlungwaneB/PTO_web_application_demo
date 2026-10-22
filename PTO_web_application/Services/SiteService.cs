using PTO_web_application.Models;
using Supabase;

namespace PTO_web_application.Services;

public class SiteService
{
    private readonly Supabase.Client _supabase;

    public SiteService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<List<Site>> GetAvailableSitesAsync()
    {
        var response = await _supabase
            .From<Site>()
            .Where(s => s.IsAvailable == true)
            .Get();

        return response.Models;
    }

    /// <summary>
    /// Get a single site by its ID.
    /// </summary>
    public async Task<Site?> GetSiteByIdAsync(Guid siteId)
    {
        var response = await _supabase
            .From<Site>()
            .Where(s => s.SiteId == siteId)
            .Limit(1)
            .Get();

        return response.Models.FirstOrDefault();
    }

    public async Task AddSiteAsync(Site site)
    {
        await _supabase.From<Site>().Insert(site);
    }

    public async Task DeleteSiteAsync(Guid siteId)
    {
        await _supabase.From<Site>().Where(s => s.SiteId == siteId).Delete();
    }

    /// <summary>All sites, available or not (the Sites screen needs both for its Available / Occupied filter).</summary>
    public async Task<List<Site>> GetAllSitesAsync()
    {
        var response = await _supabase.From<Site>().Get();
        return response.Models;
    }

    /// <summary>True when a site with the same number already exists in the same settlement.</summary>
    public async Task<bool> SiteExistsAsync(string siteName, string location)
    {
        var all = await GetAllSitesAsync();
        return all.Any(s =>
            string.Equals(s.SiteName?.Trim(), siteName.Trim(), StringComparison.OrdinalIgnoreCase) &&
            string.Equals(s.Location?.Trim(), location.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
