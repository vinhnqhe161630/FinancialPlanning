using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanning.Data.Entities
{
    [Table("position")]
    public class Position
    {
        [Key] public Guid Id { get; set; }
        [Required] public string PositionName { get; set; } = string.Empty;
        public virtual ICollection<User>? Users { get; set; }
    }
}