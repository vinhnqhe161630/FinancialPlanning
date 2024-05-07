namespace FinancialPlanning.Common;

public static class Constants
{
    public const string PasswordChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+=-";
    public const int MaxFileSize = 500 * 1024 * 1024;

    public static readonly string[] TemplatePath =
    [
        @"..\FinancialPlanning.Service\Template\Financial Plan_Template.xlsx",
        @"..\FinancialPlanning.Service\Template\Monthly Expense Report_Template.xlsx",
        @"..\FinancialPlanning.Service\Template\Annual Expense Report.xlsx"
    ];
    
    public static readonly string[][] TemplateHeader=
    [
        [
            "DATE", "TERM", "DEPARTMENT", "EXPENSE", "COST TYPE", "UNIT PRICE", "AMOUNT", "Currency", "Exchange rate",
            "TOTAL", "", "PROJECT NAME", "SUPPLIER NAME", "PIC", "NOTE"
        ],
        [
            "DATE", "TERM", "DEPARTMENT", "EXPENSE", "COST TYPE", "UNIT PRICE", "AMOUNT", "TOTAL", "PROJECT NAME",
            "SUPPLIER NAME", "PIC", "NOTE"
        ]
    ];
}