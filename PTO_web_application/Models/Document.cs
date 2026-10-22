using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PTO_web_application.Models;

[Table("documents")]
public class Document : BaseModel
{
    [PrimaryKey("document_id", false)]
    public Guid DocumentId { get; set; }

    [Column("application_id")]
    public Guid ApplicationId { get; set; }

    [Column("document_type")]
    public string DocumentType { get; set; } = string.Empty;

    [Column("file_path")]
    public string FilePath { get; set; } = string.Empty;

    [Column("verification_status")]
    public string VerificationStatus { get; set; } = "Pending";
}