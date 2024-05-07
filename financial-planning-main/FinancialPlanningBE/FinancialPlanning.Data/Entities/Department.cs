using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialPlanning.Data.Entities
{
    [Table("department")]
    public class Department
    {
        [Key] public Guid Id { get; set; }
        [Required] public string DepartmentName { get; set; } = string.Empty;
        public virtual ICollection<User>? Users { get; set; }
        public virtual ICollection<Plan>? Plans { get; set; }
        public virtual ICollection<Report>? Reports { get; set; }
    }
}