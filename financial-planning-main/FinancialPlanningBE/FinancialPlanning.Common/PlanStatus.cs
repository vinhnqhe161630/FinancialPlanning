using System.ComponentModel;

namespace FinancialPlanning.Common;

public enum PlanStatus
{
    New, 
    [Description("Waiting for Approval")]
    WaitingForApproval,
    Approved,
    Closed
}