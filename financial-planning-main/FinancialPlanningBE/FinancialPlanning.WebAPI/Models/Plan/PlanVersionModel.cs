using System.ComponentModel.DataAnnotations;
namespace FinancialPlanning.WebAPI.Models.Plan
{
    public class PlanVersionModel
    {
        public Guid Id { get; set; } 
        [Required] public int Version { get; set; }
        [Required] public Guid PlanId { get; set; }
        [Required] public DateTime ImportDate { get; set; }
        [Required] public string UploadedBy { get; set; } = string.Empty;

    }
}
