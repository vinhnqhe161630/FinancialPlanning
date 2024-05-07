

using System.ComponentModel.DataAnnotations;

namespace FinancialPlanning.WebAPI.Models.Term;
public class SelectTermModel
{
    [Required] public Guid Id { get; set; }
    [Required] public string TermName { get; set; } = string.Empty; 
    [Required] public DateTime StartDate { get; set; }
    [Required] public int Duration { get; set; }
    [Required] public DateTime ReportDueDate { get; set; }
    [Required] public DateTime PlanDueDate { get; set; }
}