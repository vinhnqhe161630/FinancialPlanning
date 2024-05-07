using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanning.Data.Entities
{
    public class AnnualReport
    {
        public int Year { get; set; }
        public DateTime CreateDate { get; set; }
        public int TotalTerm { get; set; }
        public int TotalDepartment { get; set; }
        public decimal TotalExpense { get; set; }
    }
}
