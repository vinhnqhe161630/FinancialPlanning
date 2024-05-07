using FinancialPlanning.Data.Entities;
using FinancialPlanning.Data.Repositories;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanning.Service.Services
{
    public class AnnualReportService
    {
        private readonly ITermRepository _termRepository;
        private readonly IReportRepository _reportRepository;
        private readonly FileService _fileService;

        public AnnualReportService(IReportRepository reportRepository, ITermRepository termRepository, FileService fileService)
        {
            _fileService = fileService;
            _termRepository = termRepository;
            _reportRepository = reportRepository;
        }

        public async Task GenerateAnnualReport()
        {
            try
            {
                DateTime timenow = DateTime.Now;
                DateTime create_at = new DateTime(timenow.Year, 12, 20);

                int year = timenow.Year;
                //Total term 
                int totalTerm = await _termRepository.GetTotalTermByYear(year);
                //Total department
                int totalDepartment = await _reportRepository.GetTotalDepartByYear(year);

                var expenseAnnualReports = new List<ExpenseAnnualReport>();
                //Get all reports
                List<Report> reports = await _reportRepository.GetAllReportsByYear(year);
                foreach (Report report in reports)
                {
                    string fileName = $"{report.Department.DepartmentName}/{report.Term.TermName}/{report.Month.Split(' ')[0]}/Report/version_{report.GetMaxVersion()}";

                    byte[] file = await _fileService.GetFileAsync(fileName + ".xlsx");

                    //Get expense of report
                    List<Expense> expenses = _fileService.ConvertExcelToList(file, 1);
                    if (expenses == null || !expenses.Any())
                    {
                        continue;
                    }

                    //caculate total , biigest
                    decimal totalExpense = expenses.Sum(e => e.TotalAmount);
                    decimal biggestExpense = expenses.Max(e => e.TotalAmount);
                    Expense biggestExpenseItem = expenses.FirstOrDefault(e => e.TotalAmount == biggestExpense);
                    string costType = biggestExpenseItem?.CostType ?? "Unknown";
                    ExpenseAnnualReport expenseAnnualReport = new ExpenseAnnualReport
                    {
                        Department = report.Department.DepartmentName,
                        TotalExpense = totalExpense,
                        BiggestExpenditure = biggestExpense,
                        CostType = costType,
                    };

                    //check duplicate department
                    var existingReport = expenseAnnualReports.FirstOrDefault(x => x.Department == report.Department.DepartmentName);
                    if (existingReport != null)
                    {
                        existingReport.TotalExpense += totalExpense;
                        if (existingReport.BiggestExpenditure < biggestExpense)
                        {
                            existingReport.BiggestExpenditure = biggestExpense;
                            existingReport.CostType = costType;
                        }
                    }
                    else
                    {
                        //add new row
                        expenseAnnualReports.Add(expenseAnnualReport);
                    }


                }
                //get total Expense of annual report
                var totalExpenseOflist = expenseAnnualReports.Sum(e => e.TotalExpense);

                var annualreport = new AnnualReport
                {
                    Year = year,
                    CreateDate = create_at,
                    TotalTerm = totalTerm,
                    TotalDepartment = totalDepartment,
                    TotalExpense = totalExpenseOflist
                };

                //Convert list to exel
                var annualFile = await _fileService.ConvertAnnualReportToExcel(expenseAnnualReports, annualreport);
                //Import file to cloud
                string filePath = Path.Combine("AnnualExpenseReport", "AnnualReport_" + year + ".xlsx");

                await _fileService.UploadFileAsync(filePath.Replace('\\', '/'), new MemoryStream(annualFile));

                Log.Information("Import Annual Expense Report successfully!");

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

        }

        public async Task<IEnumerable<AnnualReport>> GetAllAnnualReportsAsync()
        {
            List<AnnualReport> annualReports = new List<AnnualReport>();
            DateTime dateTime = DateTime.Now;
            DateTime timeGenAnnualReport = new DateTime(dateTime.Year, 12, 20);
            int currentYear = dateTime.Year;
            if (dateTime <= timeGenAnnualReport)
            {
                currentYear--;
            }

            while (currentYear > 0)
            {
                try
                {
                    //Get file by year
                    var file = await _fileService.GetFileAsync("AnnualExpenseReport/AnnualReport_" + currentYear + ".xlsx");

                    //Convert file to list
                    var annual = _fileService.ConvertExelToListAnnualReport(new ExcelPackage(new MemoryStream(file)));
                    annualReports.Add(annual);
                }
                catch
                {
                    //if file can't find 
                    break;
                }
                currentYear--;
            }

            return annualReports;

        }

        public async Task<(IEnumerable<ExpenseAnnualReport>, AnnualReport)> GetAnnualReportDetails(int year)
        {
            try
            {
                var file = await _fileService.GetFileAsync("AnnualExpenseReport/AnnualReport_" + year + ".xlsx");
                (var expenses, var annualreport) = _fileService.ConvertExelAnnualReportToList(new ExcelPackage(new MemoryStream(file)));

                return (expenses, annualreport);

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to process annual report details", ex);
            }


        }

        public async Task<string> GetURLFile(string name)
        {
            try
            {
                var url = await _fileService.GetFileUrlAsync(name);
                return url;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to export file :", ex);
            }

        }

    }
}
