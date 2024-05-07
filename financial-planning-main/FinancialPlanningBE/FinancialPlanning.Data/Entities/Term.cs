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
    [Table("term")]
    public class Term
    {
        [Key] public Guid Id { get; set; }
        [Required] public string TermName { get; set; } = string.Empty;
        [Required] public Guid CreatorId { get; set; }
        [Required] public int Duration { get; set; }
        [Required] public DateTime StartDate { get; set; }
        [Required] public DateTime PlanDueDate { get; set; }
        [Required] public DateTime ReportDueDate { get; set; }
        [Required] public TermStatus Status { get; set; }
        [ForeignKey("CreatorId")] public virtual User User { get; set; } = null!;
        public virtual ICollection<Plan>? Plans { get; set; }
        public virtual ICollection<Report>? Reports { get; set; }
    }
}