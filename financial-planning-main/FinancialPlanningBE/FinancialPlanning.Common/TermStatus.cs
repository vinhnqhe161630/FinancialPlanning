using System.ComponentModel;

namespace FinancialPlanning.Common;

public enum TermStatus
{
    New,
    [Description("In Progress")]
    InProgress,
    Closed
}