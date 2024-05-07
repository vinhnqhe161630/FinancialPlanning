using System.ComponentModel.DataAnnotations;
namespace FinancialPlanning.WebAPI.Models.Report
{
    public class ImportReportModel
    {
        [Required]
        public string Month { get; set; } = string.Empty;
        [Required]
        public string TermId { get; set; } = string.Empty;
        [Required]
        public string DepartmentId { get; set; } = string.Empty;

    }
}
