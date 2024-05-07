using Amazon.Runtime.Documents;
using FinancialPlanning.Common;
using AutoMapper;
using FinancialPlanning.Data.Entities;
using FinancialPlanning.Service.Services;
using FinancialPlanning.Service.Token;
using FinancialPlanning.WebAPI.Models.Department;
using FinancialPlanning.WebAPI.Models.Report;
using FinancialPlanning.WebAPI.Models.Term;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.VisualBasic;
using System.Xml.Linq;

namespace FinancialPlanning.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ReportService _reportService;
        private readonly TokenService _tokenService;
        private readonly TermService _termService;
        private readonly FileService _fileService;

        public ReportController(IMapper mapper,
            ReportService reportService, TokenService tokenService, TermService termService,
            FileService fileService
        )
        {
            _mapper = mapper;
            _reportService = reportService;
            _tokenService = tokenService;
            _termService = termService;
            _fileService = fileService;
        }

        // Phương thức để lấy danh sách báo cáo của user
        [HttpGet("reports")]
        [Authorize(Roles = "Accountant, FinancialStaff")]
        public async Task<IActionResult> GetListReport()
        {
            try
            {
                // get token from authorization header
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var useremail = _tokenService.GetEmailFromToken(token); // Get user email from JWT token

                var reports = await _reportService.GetReportsByEmail(useremail); // Get list reports          
                var departments = await _reportService.GetAllDepartment(); //Get all department
                var terms = await _termService.GetAllTerms(); //Get all term

                //Mapper model
                var reportViewModels = _mapper.Map<List<ReportViewModel>>(reports);
                var termListModels = terms.Select(t => _mapper.Map<TermListModel>(t)).ToList();
                var departmentViewModel = _mapper.Map<List<DepartmentViewModel>>(departments);

                var result = new
                {
                    Reports = reportViewModels,
                    Terms = termListModels,
                    Departments = departmentViewModel
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Accountant, FinancialStaff")]
        public async Task<IActionResult> DeleteReport(Guid id)
        {
            try
            {
                await _reportService.DeleteReport(id);
                return Ok(new { message = $"Report with id {id} deleted successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("details/{id:guid}")]
        [Authorize(Roles = "Accountant, FinancialStaff")]
        public async Task<IActionResult> GetReportDetails(Guid id)
        {
            try
            {
                //Get report
                var report = await _reportService.GetReportById(id);
                string filename = $"{report!.Department.DepartmentName}/{report.Term.TermName}/" +
                    $"{report.Month.Split(' ')[0]}/Report/version_{report.GetMaxVersion()}.xlsx";

                //Get reportVersions
                var reportVersions = await _reportService.GetReportVersionsAsync(id);

                //Download file report form cloud
                var expenses = _fileService.ConvertExcelToList(await _fileService.GetFileAsync(filename), 1);

                //mapper
                var reportViewModel = _mapper.Map<ReportViewModel>(report);
                var reportVersionModel = _mapper.Map<IEnumerable<ReportVersionModel>>(reportVersions).ToList();
                // Get the name of the user who uploaded the file
                var firstReportVersion = reportVersionModel.FirstOrDefault();
                var uploadedBy = firstReportVersion?.UploadedBy;

                var result = new
                {
                    Report = reportViewModel,
                    Expenses = expenses,
                    ReportVersions = reportVersionModel,
                    UploadedBy = uploadedBy
                };

                return Ok(result);
            }
            //error when download
            catch (Exception ex)
            {
                return StatusCode(404, $"Error : {ex.Message}");
            }
        }

        //export report 
        [HttpPost("export/{id:guid}/{version:int}")]
        [Authorize(Roles = "Accountant, FinancialStaff")]
        public async Task<IActionResult> ExportSingleReport(Guid id, int version)
        {
            try
            {
                //from reportVersion Id -> get name report + version
                var report = await _reportService.GetReportById(id);
                string filename = $"{report!.Department.DepartmentName}/{report.Term.TermName}/{report.Month.Split(' ')[0]}/Report/version_{version}.xlsx";


                //get file
                var reports = await _reportService.GetFileByName(filename);
                reports = _fileService.AddNoColumn(reports, _fileService.ConvertExcelToList(reports, 1));
                reports = _fileService.RemoveFirstRow(reports);

                return File(reports, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
              report.ReportName+".xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex });
            }
        }

        [HttpPost("export")]
        [Authorize(Roles = "Accountant, FinancialStaff")]
        public async Task<IActionResult> ExportMultipleReport(List<Guid> reportIds)
        {
            var reports = await _reportService.MergeExcelFiles(reportIds);
            reports = _fileService.RemoveFirstRow(reports);

            return File(reports, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                DateTime.Now.ToString("ddMMyyyyHHmmss") + "_reports.xlsx");
        }

        [HttpPost("import")]
        [Authorize(Roles = "Accountant, FinancialStaff")]
        public async Task<IActionResult> ImportReport(IFormFile file)
        {
            try
            {
                if (file.Length == 0)
                {
                    return BadRequest(new { message = "File empty" });
                }

                using MemoryStream ms = new();
                await file.CopyToAsync(ms);
                var fileBytes = ms.ToArray();
                string isValid = _reportService.ValidateReportFile(fileBytes);

                if (!String.IsNullOrEmpty(isValid))
                {
                    return BadRequest(new { message = isValid });
                }

                var expenses = _reportService.GetExpenses(fileBytes);

                return Ok(expenses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex });
            }
        }

        [HttpPost("upload")]
        [Authorize(Roles = "Accountant, FinancialStaff")]
        public async Task<IActionResult> UploadReport(List<Expense> expenses, Guid termId, Guid uid, string month)
        {
            try
            {
                var report = new Report
                {
                    TermId = termId,
                    Month = month
                };
                await _reportService.CreateReport(expenses, report, uid);
                return Ok(new { message = "Report uploaded successfully!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex });
            }
        }

        [HttpPost("reupload")]
        [Authorize(Roles = "Accountant, FinancialStaff")]
        public async Task<IActionResult> ReuploadReport(List<Expense> expenses, Guid reportId, Guid uid)
        {
            try
            {
                await _reportService.ReupReport(expenses, reportId, uid);
                return Ok(new { message = "Report reuploaded successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex });
            }
        }

        [HttpGet]
        [Route("exportTemplate")]
        public async Task<IActionResult> ExportTemplate()
        {
            var filepath = Path.Combine(Directory.GetCurrentDirectory(), Common.Constants.TemplatePath[1]);

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filepath, out var contenttype))
            {
                contenttype = "application/octet-stream";
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(filepath);
            return File(bytes, contenttype, Path.GetFileName(filepath));
        }
    }
}