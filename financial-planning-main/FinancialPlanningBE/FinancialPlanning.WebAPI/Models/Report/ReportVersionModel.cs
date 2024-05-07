using System.ComponentModel.DataAnnotations;
namespace FinancialPlanning.WebAPI.Models.Report
{
    public class ReportVersionModel
    {
        public Guid Id { get; set; } 
        [Required] public int Version { get; set; }
        [Required] public Guid ReportId { get; set; }
        [Required] public DateTime ImportDate { get; set; }
        [Required] public string UploadedBy { get; set; } = string.Empty;

    }
}
