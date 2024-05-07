using System;
using FinancialPlanning.Common;

namespace FinancialPlanning.WebAPI.Models.Expense
{
    public class ExpenseStatusModel
    {
        public int No { get; set; }
        public DateTime Date { get; set; }
        public string Term { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string ExpenseName { get; set; } = string.Empty;
        public string CostType { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public double? ExchangeRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string PIC { get; set; } = string.Empty;
        public string? Note { get; set; }
        public PlanStatus Status { get; set; }
    }
}