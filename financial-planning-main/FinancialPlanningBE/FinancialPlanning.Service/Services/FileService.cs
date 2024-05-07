using System.Data;
using System.Globalization;
using Amazon.Runtime.Documents;
using Amazon.S3;
using Amazon.S3.Model;
using Aspose.Cells;
using FinancialPlanning.Common;
using FinancialPlanning.Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic.FileIO;
using OfficeOpenXml;
using Serilog;
using LoadOptions = System.Xml.Linq.LoadOptions;

namespace FinancialPlanning.Service.Services;

public class FileService(IAmazonS3 s3Client, IConfiguration configuration)
{


    public async Task<string> GetFileUrlAsync(string key)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = configuration["AWS:BucketName"],
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(15)
        };

        return await s3Client.GetPreSignedURLAsync(request);
    }

    public async Task<byte[]> GetFileAsync(string key)
    {
        using var response = await s3Client.GetObjectAsync(configuration["AWS:BucketName"], key);

        await using var stream = response.ResponseStream;
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        return memoryStream.ToArray();
    }

    public async Task DeleteFileAsync(string key)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = configuration["AWS:BucketName"],
            Key = key,

        };

        await s3Client.DeleteObjectAsync(request);
    }

    /*
     * documentType:
     * {
     *  plan:   0
     *  report: 1
     * }
     */
    public async Task UploadFileAsync(string key, Stream fileStream)
    {
        var request = new PutObjectRequest
        {
            BucketName = configuration["AWS:BucketName"],
            Key = key,
            InputStream = fileStream,
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

        await s3Client.PutObjectAsync(request);
    }
    /*
     * documentType:
     * {
     *  plan:   0
     *  report: 1
     * }
     */
    public string ValidateFile(byte[] file, byte documentType)
    {
        string mess = String.Empty;
        //check file is not empty and not bigger than 500MB 
        if (file.Length is 0 or > Constants.MaxFileSize)
        {
            mess += "File is empty or bigger than 500MB;";

        }

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var package = new ExcelPackage(new MemoryStream(file));
        package = RemoveEmptyRow(package);

        var worksheet = package.Workbook.Worksheets[0];
        var numOfRows = worksheet.Dimension?.Rows ?? 0;

        //check number of column is sufficient
        var numOfColumns = worksheet.Dimension?.Columns ?? 0;
        if (numOfColumns != Constants.TemplateHeader[documentType].Length)
             mess+= "number of column is sufficient;";

        //remove empty row (all cell in row is empty)

        //check file has data
        if (numOfRows < 3)
            mess += "file hasn't data;";

        //check cell content is not null and has valid format
        for (var i = 1; i <= numOfColumns; i++)
        {
            if (!(worksheet.Cells[2, i].Value?.ToString() ?? "").Equals(Constants.TemplateHeader[documentType][i - 1]))
                mess+= $"cell {2}, {i} content is null and has invalid format;";

            for (var j = 3; j <= numOfRows; j++)
            {
                var cellContext = worksheet.Cells[j, i].Value?.ToString()?.Trim();
                if ((documentType == 0 ? i != 11 && i != 15 : i != 12) && cellContext.IsNullOrEmpty())
                    mess+= $"cell {j}, {i} content is null and has invalid format;";

                switch (i)
                {
                    case 1:
                        if (!DateTime.TryParseExact(cellContext!, ["dd/MM/yyyy", "dd/MM/yyyy hh:mm:ss tt"], null,
                                DateTimeStyles.None, out _))
                            mess += $"cell {j}, {i} content is null and has invalid dateformat;";
                        break;
                    case 6:
                    case 7:
                    case 8 when documentType != 0:
                    case 10 when documentType == 0:
                        if (!decimal.TryParse(cellContext!, NumberStyles.Currency, null, out _))
                            mess += $"cell {j}, {i} content is null and has invalid numberformat;";;
                        break;
                }
            }
        }

        //check TotalAmount = UnitPrice * Amount
        for (var i = 3; i < numOfRows; i++)
        {
            var unitPrice = decimal.Parse(worksheet.Cells[i, 6].Value.ToString()!);
            var amount = decimal.Parse(worksheet.Cells[i, 7].Value.ToString()!);
            var totalAmount = decimal.Parse(worksheet.Cells[i, documentType != 0 ? 8 : 10].Value.ToString()!);
            if (totalAmount != unitPrice * amount)
                mess+= "TotalAmount != UnitPrice * Amount;";
        }

        return mess;
    }

    /*
     * documentType:
     * {
     *  plan:   0
     *  report: 1
     * }
     */
    public List<Expense> ConvertExcelToList(byte[] file, byte documentType)
    {
        try
        {
            var expenses = new List<Expense>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage(new MemoryStream(file));
            package = RemoveEmptyRow(package);

            var worksheet = package.Workbook.Worksheets[0];
            for (var i = 3; i <= worksheet.Dimension.Rows; i++)
            {
                expenses.Add(new Expense
                {
                    No = i - 2,
                    Date = DateTime.Parse(worksheet.Cells[i, 1].Value.ToString()!.Trim()),
                    Term = worksheet.Cells[i, 2].Value.ToString()!.Trim(),
                    Department = worksheet.Cells[i, 3].Value.ToString()!.Trim(),
                    ExpenseName = worksheet.Cells[i, 4].Value.ToString()!.Trim(),
                    CostType = worksheet.Cells[i, 5].Value.ToString()!.Trim(),
                    UnitPrice = decimal.Parse(worksheet.Cells[i, 6].Value.ToString()!.Trim()),
                    Amount = decimal.Parse(worksheet.Cells[i, 7].Value.ToString()!.Trim()),
                    Currency = documentType == 0 ? worksheet.Cells[i, 8].Value.ToString()!.Trim() : null,
                    ExchangeRate = documentType == 0
                        ? double.Parse(worksheet.Cells[i, 9].Value.ToString()!.Trim(), CultureInfo.InvariantCulture)
                        : null,
                    TotalAmount = decimal.Parse(worksheet.Cells[i, 10 - documentType * 2].Value.ToString()!.Trim()),
                    ProjectName = worksheet.Cells[i, 12 - documentType * 3].Value.ToString()!.Trim(),
                    SupplierName = worksheet.Cells[i, 13 - documentType * 3].Value.ToString()!.Trim(),
                    PIC = worksheet.Cells[i, 14 - documentType * 3].Value.ToString()!.Trim(),
                    Note = worksheet.Cells[i, 15 - documentType * 3].Value?.ToString()?.Trim()
                });
            }

            return expenses;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while converting Excel to list.");
            throw; // Re-throw exception to propagate it to the caller
        }
    }

    public async Task<byte[]> ConvertListToExcelAsync(IEnumerable<Expense> expenses, byte documentType)
    {
        try
        {
            //Write list of expenses to ExcelPackage
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package =
                new ExcelPackage(new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), Constants.TemplatePath[documentType])));
            var worksheet = package.Workbook.Worksheets[0];
            foreach (var (expense, index) in expenses.Select((value, i) => (value, i)))
            {
                worksheet.Cells[index + 3, 1].Value = expense.Date.ToString().Substring(0, 10);
                worksheet.Cells[index + 3, 2].Value = expense.Term;
                worksheet.Cells[index + 3, 3].Value = expense.Department;
                worksheet.Cells[index + 3, 4].Value = expense.ExpenseName;
                worksheet.Cells[index + 3, 5].Value = expense.CostType;
                worksheet.Cells[index + 3, 6].Value = expense.UnitPrice;
                worksheet.Cells[index + 3, 7].Value = expense.Amount;

                //if document is plan
                if (documentType == 0)
                {
                    worksheet.Cells[index + 3, 8].Value = expense.Currency;
                    worksheet.Cells[index + 3, 9].Value = expense.ExchangeRate;
                }

                worksheet.Cells[index + 3, 10 - documentType * 2].Value = expense.TotalAmount;
                worksheet.Cells[index + 3, 12 - documentType * 3].Value = expense.ProjectName;
                worksheet.Cells[index + 3, 13 - documentType * 3].Value = expense.SupplierName;
                worksheet.Cells[index + 3, 14 - documentType * 3].Value = expense.PIC;
                worksheet.Cells[index + 3, 15 - documentType * 3].Value = expense.Note;
            }

            //Convert ExcelPackage to Stream
            var memoryStream = new MemoryStream();
            await package.SaveAsAsync(memoryStream);

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while converting list to Excel.");
            throw; // Re-throw exception to propagate it to the caller
        }
    }

    private ExcelPackage RemoveEmptyRow(ExcelPackage package)
    {
        var worksheet = package.Workbook.Worksheets[0];

        for (var i = 1; i <= (worksheet.Dimension?.Rows ?? 0); i++)
        {
            var isEmptyRow = true;

            for (var j = 1; j <= (worksheet.Dimension?.Columns ?? 0); j++)
                if (!string.IsNullOrEmpty(worksheet.Cells[i, j].Value?.ToString()?.Trim()))
                {
                    isEmptyRow = false;
                    break;
                }

            if (!isEmptyRow) continue;
            worksheet.DeleteRow(i--);
        }

        package.Save();

        return package;
    }

    //Convert file exel of annual report to list expense and AnnualReport
    public (List<ExpenseAnnualReport>, AnnualReport) ConvertExelAnnualReportToList(ExcelPackage package)
    {
        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();

        if (worksheet != null)
        {
            List<ExpenseAnnualReport> expense = new List<ExpenseAnnualReport>();


            AnnualReport report = new AnnualReport
            {
                Year = int.Parse(worksheet.Cells["B1"].Value?.ToString()),
                CreateDate = DateTime.Parse(worksheet.Cells["B2"].Value?.ToString()),
                TotalTerm = int.Parse(worksheet.Cells["B3"].Value?.ToString()),
                TotalDepartment = int.Parse(worksheet.Cells["B4"].Value?.ToString()),
                TotalExpense = decimal.Parse(worksheet.Cells["B5"].Value?.ToString()),
            };
            for (int row = 8; row <= worksheet.Dimension.End.Row; row++)
            {

                expense.Add(new ExpenseAnnualReport
                {
                    Department = worksheet.Cells[row, 1].Value?.ToString(),
                    TotalExpense = decimal.Parse(worksheet.Cells[row, 2].Value?.ToString()),
                    BiggestExpenditure = decimal.Parse(worksheet.Cells[row, 3].Value?.ToString()),
                    CostType = worksheet.Cells[row, 4].Value?.ToString()
                });

            }

            return (expense, report);
        }

        throw new Exception("Invalid Excel file.");
    }

    //Convert file exel of annual report to list  AnnualReport
    public AnnualReport ConvertExelToListAnnualReport(ExcelPackage package)
    {
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet != null)
            {

                AnnualReport report = new AnnualReport
                {
                    Year = int.Parse(worksheet.Cells["B1"].Value?.ToString()),
                    CreateDate = DateTime.Parse(worksheet.Cells["B2"].Value?.ToString()),
                    TotalTerm = int.Parse(worksheet.Cells["B3"].Value?.ToString()),
                    TotalDepartment = int.Parse(worksheet.Cells["B4"].Value?.ToString()),
                    TotalExpense = decimal.Parse(worksheet.Cells["B5"].Value?.ToString())
                };


                return report;
            }

            throw new Exception("Invalid Excel file.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while converting Excel to annual report list.");
            throw; // Re-throw exception to propagate it to the caller
        }
    }

    //Convert list to annual report file exel
    public async Task<byte[]> ConvertAnnualReportToExcel(List<ExpenseAnnualReport> expenses, AnnualReport report)
    {
        try
        {
            //Write list of expenses to ExcelPackage
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package =
                new ExcelPackage(new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), Constants.TemplatePath[2])));
            var worksheet = package.Workbook.Worksheets[0];

            //Annual report
            worksheet.Cells["B1"].Value = report.Year;
            worksheet.Cells["B2"].Value = report.CreateDate.ToString();
            worksheet.Cells["B3"].Value = report.TotalTerm;
            worksheet.Cells["B4"].Value = report.TotalDepartment;
            worksheet.Cells["B5"].Value = report.TotalExpense;

            int row = 8;
            foreach (var expense in expenses)
            {
                worksheet.Cells[row, 1].Value = expense.Department;
                worksheet.Cells[row, 2].Value = expense.TotalExpense;
                worksheet.Cells[row, 3].Value = expense.BiggestExpenditure;
                worksheet.Cells[row, 4].Value = expense.CostType;
                row++;
            }

            //Convert ExcelPackage to Stream
            var memoryStream = new MemoryStream();
            await package.SaveAsAsync(memoryStream);

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while converting report list to annual Excel.");
            throw; // Re-throw exception to propagate it to the caller
        }
    }

    public MemoryStream ConvertCsvToExcel(MemoryStream csvStream)
    {
        var workbook = new Workbook(csvStream, new TxtLoadOptions(LoadFormat.CSV)
        {
            Separator = ';',
            ConvertDateTimeData = false
        });

        var excelStream = new MemoryStream();
        workbook.Save(excelStream, SaveFormat.Xlsx);

        return excelStream;
    }

    public byte[] AddNoColumn(byte[] file, List<Expense> expenses)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var package = new ExcelPackage(new MemoryStream(file));
        package = RemoveEmptyRow(package);
        
        var worksheet = package.Workbook.Worksheets[0];
        worksheet.InsertColumn(1, 1);

        worksheet.Cells[2, 1].Value = "NO.";
        for (var row = 3; row <= worksheet.Dimension.Rows; row++)
        {
            worksheet.Cells[row, 1].Value = expenses[row-3].No;
        }
        
        using var memoryStream = new MemoryStream();
        package.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }
    
    public byte[] RemoveFirstRow(byte[] file)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var package = new ExcelPackage(new MemoryStream(file));
        
        var worksheet = package.Workbook.Worksheets[0];
        worksheet.DeleteRow(1);
        
        using var memoryStream = new MemoryStream();
        package.SaveAs(memoryStream);
        return memoryStream.ToArray();
    } 
    
    public byte[] AddStatusColumn(byte[] file, bool isSubmitted, List<int> approvedExpenses)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var package = new ExcelPackage(new MemoryStream(file));
        package = RemoveEmptyRow(package);
        
        var worksheet = package.Workbook.Worksheets[0];
        worksheet.InsertColumn(17, 1);

        worksheet.Cells[2, 17].Value = "STATUS";
        for (var row = 3; row <= worksheet.Dimension.Rows; row++)
        {
            worksheet.Cells[row, 17].Value = isSubmitted ? approvedExpenses.Contains(row) ? "Approved" : "Waiting for approval" : "New";
        }
        
        using var memoryStream = new MemoryStream();
        package.SaveAsAsync(memoryStream);
        return memoryStream.ToArray();
    }
}