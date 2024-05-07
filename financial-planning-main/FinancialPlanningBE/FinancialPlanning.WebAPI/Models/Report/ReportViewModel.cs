using FinancialPlanning.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinancialPlanning.WebAPI.Models.Report
{
    public class ReportViewModel
    {
        public Guid Id { get; set; }
        [Required] public string ReportName { get; set; } = string.Empty;
        [Required] public string Month { get; set; } = string.Empty;
        [Required] public int Status { get; set; }
        [Required] public string TermName { get; set; } = null!;
        [Required] public DateTime UpdateDate { get; set; }
        [Required] public string DepartmentName { get; set; } = null!;
        [Required] public string Version { get; set; } = null!;
        [Required] public DateTime ReportDureDate { get; set; }
       

    }
}
