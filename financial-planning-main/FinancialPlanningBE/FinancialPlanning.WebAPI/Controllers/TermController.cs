using AutoMapper;
using FinancialPlanning.Service.Services;
using FinancialPlanning.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using FinancialPlanning.WebAPI.Models.Term;
using System.Security.Claims;
using FinancialPlanning.Common;
using FluentAssertions;

namespace FinancialPlanning.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermController(IMapper mapper, TermService termService) : ControllerBase
    {
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly TermService _termService = termService ?? throw new ArgumentNullException(nameof(termService));

        [HttpPost]
        [Authorize(Roles = "Accountant")]
        public async Task<IActionResult> CreateTerm(CreateTermModel termModel)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(new { error = "Invalid model state!" });
                var term = _mapper.Map<Term>(termModel);
                await _termService.CreateTerm(term);
                return Ok(new { message = "Term created successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("start/{id:guid}")]
        [Authorize(Roles = "Accountant")]
        public async Task<IActionResult> StartTerm(Guid id)
        {
            try
            {
                await _termService.StartTerm(id);
                return Ok(new { message = "Term started successfully!" });
            }
            catch (ArgumentNullException)
            {
                return NotFound("Term not found with the specified ID");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Accountant, FinancialStaff")]
        public async Task<IActionResult> GetTermById(Guid id)
        {
            try
            {
                var term = await _termService.GetTermByIdAsync(id);
                if (term == null) return NotFound(new { error = "Term not found!" });
                var termViewModel = _mapper.Map<TermViewModel>(term);
                return Ok(termViewModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "Accountant, FinancialStaff")]
        public async Task<IActionResult> GetAllTerms()
        {
            try
            {
                var terms = await _termService.GetAllTerms();
                if (terms == null) return NotFound(new { error = "No terms found!" });
                var role = User.FindFirst(ClaimTypes.Role)!.Value;
                if (role == "FinancialStaff")
                {
                    terms = terms.Where(t => t.Status != TermStatus.New);
                }
                var termListModels = terms.Select(_mapper.Map<TermListModel>).ToList().OrderByDescending(t => t.StartDate);
                return Ok(termListModels);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("update/{id:guid}")]
        [Authorize(Roles = "Accountant")]
        public async Task<IActionResult> UpdateTerm(Guid id, CreateTermModel termModel)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(new { error = "Invalid model state!" });
                var term = _mapper.Map<Term>(termModel);
                term.Id = id;
                await _termService.UpdateTerm(term);
                return Ok(new { message = $"Term with id {id} updated successfully!" });
            }
            catch (ArgumentNullException)
            {
                return NotFound("Term not found with the specified ID");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Accountant")]
        public async Task<IActionResult> DeleteTerm(Guid id)
        {
            try
            {
                await _termService.DeleteTerm(id);
            }
            catch (ArgumentNullException)
            {
                return NotFound("Term not found with the specified ID");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
 
            return Ok(new { message = $"Term with id {id} deleted successfully!" });
        }

        [HttpGet("noplan")]
        [Authorize(Roles = "Accountant, FinancialStaff")]
        public async Task<IActionResult> GetTermsToImportPlan()
        {
            var userId = Guid.Parse(User.FindFirst("userId")!.Value);
            var terms = await _termService.GetTermsWithNoPlanByUserId(userId);

            var selectTermModels = _mapper.Map<List<SelectTermModel>>(terms);
            return Ok(selectTermModels);
        }

        [HttpGet("noreport")]
        [Authorize(Roles = "Accountant, FinancialStaff")]
        public async Task<IActionResult> GetTermsToImportReport()
        {
            var userId = Guid.Parse(User.FindFirst("userId")!.Value);
            var terms = await _termService.GetTermsWithUnFullFilledReports(userId);

            var selectTermModels = _mapper.Map<List<SelectTermModel>>(terms);
            return Ok(selectTermModels);
        }

        [HttpPut("close/{id:guid}")]
        [Authorize(Roles = "Accountant")]
        public async Task<IActionResult> CloseTerm(Guid id)
        {
            try {
                await _termService.CloseTerm(id);
                return Ok(new { message = "Term closed successfully!" });
            } catch (ArgumentNullException)
            {
                return NotFound("Term not found with the specified ID");
            } catch (Exception ex) {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}