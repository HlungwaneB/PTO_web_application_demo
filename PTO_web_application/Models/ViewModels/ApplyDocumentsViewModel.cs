using Microsoft.AspNetCore.Http;

namespace PTO_web_application.Models.ViewModels;

public class ApplyDocumentsViewModel
{
    public IFormFile? CertifiedId { get; set; }
    public IFormFile? ProofOfIncome { get; set; }
    public IFormFile? DependentId { get; set; }
}