using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PTO_web_application.Models;

[Table("applications")]
public class Application : BaseModel
{
    [PrimaryKey("application_id", false)]
    public Guid ApplicationId { get; set; }

    [Column("id_number")]
    public string IdNumber { get; set; } = string.Empty;

    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Column("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("site_id")]
    public Guid SiteId { get; set; }

    [Column("pto_type")]
    public string PtoType { get; set; } = "Residential";

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("current_status")]
    public string CurrentStatus { get; set; } = "Pending";

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    // 🟢 ADD THESE 3 LINES TO STORE DOCUMENT URLS
    [Column("certified_id_url")]
    public string? CertifiedIdUrl { get; set; }

    [Column("proof_of_income_url")]
    public string? ProofOfIncomeUrl { get; set; }

    [Column("dependent_id_url")]
    public string? DependentIdUrl { get; set; }
}