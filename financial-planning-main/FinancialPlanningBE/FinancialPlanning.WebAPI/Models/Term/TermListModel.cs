using System.ComponentModel.DataAnnotations;

namespace FinancialPlanning.WebAPI.Models.Term;

public class TermListModel
{
    [Required] public Guid Id { get; set; }
    [Required] public string TermName { get; set; } = string.Empty;
    [Required] public DateTime StartDate { get; set; }
    [Required] public DateTime EndDate { get; set; }
    [Required] public string Status { get; set; } = string.Empty;
}