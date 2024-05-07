namespace FinancialPlanning.WebAPI.Models.Plan
{
    public class PlanViewModel
    {
        public Guid Id { get; set; }
        public string Plan { get; set; } = String.Empty; 
        public string Term { get; set; } = String.Empty;
        public string Department { get; set; } = String.Empty;
        public string Status { get; set; } = String.Empty;
        public int Version { get; set; }
        public DateTime DueDate { get; set; }
        public string ApprovedExpenses { get; set; } = string.Empty;

    }
}
