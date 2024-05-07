using FinancialPlanning.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanning.Data.Entities
{
    [Table("user")]
    public class User
    {
        [Key] public Guid Id { get; set; }
        [Required] public string Username { get; set; } = string.Empty;
        [Required] public string FullName { get; set; } = string.Empty;

        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required] public string Address { get; set; } = string.Empty;
        [Required] public string DOB { get; set; } = string.Empty;
        [Required] public Guid DepartmentId { get; set; }
        [Required] public Guid PositionId { get; set; }
        [Required] public Guid RoleId { get; set; }
        public string? Token { get; set; }
        [Required] public UserStatus Status { get; set; }
        public string? Notes { get; set; }
        [ForeignKey("DepartmentId")] public virtual Department Department { get; set; } = null!;
        [ForeignKey("PositionId")] public virtual Position Position { get; set; } = null!;
        [ForeignKey("RoleId")] public virtual Role Role { get; set; } = null!;
        public virtual ICollection<Term>? Terms { get; set; }
        public virtual ICollection<PlanVersion>? PlanVersions { get; set; }
        public virtual ICollection<ReportVersion>? ReportVersions { get; set; }
    }
}