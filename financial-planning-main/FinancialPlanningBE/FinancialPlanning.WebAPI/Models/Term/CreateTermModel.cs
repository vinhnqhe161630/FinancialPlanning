

using System.ComponentModel.DataAnnotations;

namespace FinancialPlanning.WebAPI.Models.Term;
public class CreateTermModel
{
    [Required(ErrorMessage = "Term name is required")]
    public string TermName { get; set; } = null!;

    [Required(ErrorMessage = "Creator ID is required")]
    public Guid CreatorId { get; set; }

    [Required(ErrorMessage = "Duration is required")]
    [RegularExpression("^[1,3,6]$", ErrorMessage = "Duration must be set to 1, 3, or 6")]
    public int Duration { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Plan due date is required")]
    public DateTime PlanDueDate { get; set; }

    [Required(ErrorMessage = "Report due date is required")]
    public DateTime ReportDueDate { get; set; }
    
}