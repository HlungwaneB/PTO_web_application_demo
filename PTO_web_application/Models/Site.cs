using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PTO_web_application.Models;

[Table("sites")]
public class Site : BaseModel
{
    [PrimaryKey("site_id", false)]
    public Guid SiteId { get; set; }

    [Column("site_name")]
    public string SiteName { get; set; } = string.Empty;

    [Column("location")]
    public string Location { get; set; } = string.Empty;

    [Column("size_details")]
    public string? SizeDetails { get; set; }

    [Column("fee_amount")]
    public decimal FeeAmount { get; set; }

    [Column("is_available")]
    public bool IsAvailable { get; set; } = true;
}