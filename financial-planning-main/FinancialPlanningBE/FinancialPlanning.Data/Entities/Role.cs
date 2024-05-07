using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialPlanning.Data.Entities
{
    [Table("role")]
    public class Role
    {
        [Key] public Guid Id { get; set; }
        [Required] public string RoleName { get; set; } = string.Empty;
        public virtual ICollection<User>? Users { get; set; }
    }
}