using System.ComponentModel.DataAnnotations;

namespace FinancialPlanning.WebAPI.Models.Position
{
    public class PositionViewModel
    {
        [Required] public Guid Id { get; set; }
        [Required] public string PositionName { get; set; } = string.Empty;
    }
}
