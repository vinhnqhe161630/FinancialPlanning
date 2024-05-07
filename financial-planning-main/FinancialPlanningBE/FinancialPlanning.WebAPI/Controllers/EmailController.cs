using FinancialPlanning.Service.DTOs;
using FinancialPlanning.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanning.WebAPI.Controllers
{
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;
        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }
        [HttpPost("send")]
        public IActionResult SendEmail(EmailDto request)
        {
            _emailService.SendEmail(request);
            return Ok("Email sent successfully");
        }
    }
}