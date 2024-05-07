using System.ComponentModel.DataAnnotations;

namespace FinancialPlanning.WebAPI.Models.Role
{
    public class RoleViewModel
    {
        [Required] public Guid Id { get; set; }
        [Required] public string RoleName { get; set; } = string.Empty;
    }
}
