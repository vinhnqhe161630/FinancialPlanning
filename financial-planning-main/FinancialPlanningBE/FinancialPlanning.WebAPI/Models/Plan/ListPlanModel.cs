using System.ComponentModel.DataAnnotations;
using FinancialPlanning.Data.Entities;

namespace FinancialPlanning.WebAPI.Models.Plan
{
    public class ListPlanModel
    {
        [Required] public Guid Id { get; set; }
        [Required] public string PlanName { get; set; } = string.Empty;
        [Required] public int Status { get; set; }
        [Required] public Guid TermId { get; set; }
        [Required] public Guid DepartmentId { get; set; }
    }
}
