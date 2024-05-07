using System.ComponentModel.DataAnnotations;

namespace FinancialPlanning.WebAPI.Models.Term
{
    public class TermViewModel(Guid id, string termName, Guid creatorId, int duration, DateTime startDate, DateTime planDueDate, DateTime reportDueDate, string status)
    {
       
        public Guid Id { get; set; } = id;

        [Required(ErrorMessage = "Term name is required")]
        public string TermName { get; set; } = termName;

        [Required(ErrorMessage = "Creator ID is required")]
        public Guid CreatorId { get; set; } = creatorId;

        [Required(ErrorMessage = "Duration is required")]
        [RegularExpression("^[1,3,6]$", ErrorMessage = "Duration must be set to 1, 3, or 6")]
        public int Duration { get; set; } = duration;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; } = startDate;

        [Required(ErrorMessage = "Plan due date is required")]
        public DateTime PlanDueDate { get; set; } = planDueDate;

        [Required(ErrorMessage = "Report due date is required")]
        public DateTime ReportDueDate { get; set; } = reportDueDate;

        // [Required(ErrorMessage = "Status is required")]
        // [Range(1, 3, ErrorMessage = "Status must be set to 1, 2, or 3")]
        public string Status { get; } = status;
    }
}
