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
}