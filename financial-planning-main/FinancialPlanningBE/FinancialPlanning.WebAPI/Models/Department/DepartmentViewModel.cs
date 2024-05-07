using FinancialPlanning.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace FinancialPlanning.WebAPI.Models.Department
{
    public class DepartmentViewModel
    {
        [Required] public Guid Id { get; set; }
        [Required] public string DepartmentName { get; set; } = string.Empty;
       
    }
}
