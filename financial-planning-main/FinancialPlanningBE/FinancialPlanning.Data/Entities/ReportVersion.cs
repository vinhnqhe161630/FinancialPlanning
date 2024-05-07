using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanning.Data.Entities
{
    [Table("report_version")]
    public class ReportVersion
    {
        [Key] public Guid Id { get; set; }
        [Required] public Guid ReportId { get; set; }
        [Required] public int Version { get; set; }
        [Required] public DateTime ImportDate { get; set; }
        [Required] public Guid CreatorId { get; set; }
        [ForeignKey("ReportId")] public virtual Report Report { get; set; } = null!;
        [ForeignKey("CreatorId")] public virtual User User { get; set; } = null!;
    }
}