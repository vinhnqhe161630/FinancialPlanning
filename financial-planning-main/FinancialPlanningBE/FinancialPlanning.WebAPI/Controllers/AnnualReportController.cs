using Amazon.S3;
using FinancialPlanning.Data.Entities;
using FinancialPlanning.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Globalization;

namespace FinancialPlanning.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnualReportController : ControllerBase
    {

        private readonly FileService _fileService;
        private readonly AnnualReportService _annualReportService;
        public AnnualReportController(FileService fileService, AnnualReportService annualReportService)
        {
            _fileService = fileService;
            _annualReportService = annualReportService;
        }

        [HttpGet("annualreports")]
        [Authorize(Roles = "Accountant, FinancialStaff")]
        public async Task<IActionResult> GetAll()
        {
            var annualReport = await _annualReportService.GetAllAnnualReportsAsync();
            return Ok(annualReport);

        }

        [HttpGet("details/{year:int}")]
        [Authorize(Roles = "Accountant, FinancialStaff")]
        public async Task<IActionResult> GetAnnualReportDetails(int year)
        {
            try
            {
                var (expenses, report) = await _annualReportService.GetAnnualReportDetails(year);
                return Ok(new { Expenses = expenses, Report = report });
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("export/{year:int}")]
        [Authorize(Roles = "Accountant, FinancialStaff")]
        public async Task<IActionResult> ExportAnnualReport(int year)
        {
            try
            {
                string filename = "AnnualExpenseReport/AnnualReport_" + year + ".xlsx";
                var url = _annualReportService.GetURLFile(filename);
                return Ok(url);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }





    }




}
