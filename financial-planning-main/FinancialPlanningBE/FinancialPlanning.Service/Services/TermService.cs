using FinancialPlanning.Common;
using FinancialPlanning.Data.Entities;
using FinancialPlanning.Data.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Serilog;

namespace FinancialPlanning.Service.Services
{
    public class TermService
    {
        private readonly ITermRepository _termRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IPlanRepository _planRepository;
        private readonly IReportRepository _reportRepository;
        public TermService(ITermRepository termRepository, IDepartmentRepository departmentRepository, IPlanRepository planRepository, IReportRepository reportRepository)
        {
            _termRepository = termRepository;
            _departmentRepository = departmentRepository;
            _planRepository = planRepository;
            _reportRepository = reportRepository;
        }

        public async Task<IEnumerable<Term>> GetTermsToStart()
        {
            IEnumerable<Term> terms = await _termRepository.GetAllTerms();
            List<Term> startingTerms = [];
            foreach (var term in terms)
            {

                if (term.StartDate.AddDays(-7) <= DateTime.Now && (int)term.Status == (int)TermStatus.New)
                {
                    startingTerms.Add(term);
                }
            }
            return startingTerms;
        }

        public async Task<Term?> GetTermByIdAsync(Guid id)
        {
            // var term = await _termRepository.GetTermByIdAsync(id);
            return await _termRepository.GetTermByIdAsync(id);
        }

        public Term GetTermById(Guid id)
        {
            return _termRepository.GetTermById(id);
        }
        public async Task StartTerm(Guid id)
        {
            var term = await _termRepository.GetTermByIdAsync(id);
            if (term != null)
            {
                term.Status = TermStatus.InProgress;
                await _termRepository.UpdateTerm(term);
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        public async Task CreateTerm(Term term)
        {
            term.Status = TermStatus.New;
            var endDate = term.StartDate.AddMonths(term.Duration);
            if (endDate < term.ReportDueDate)
            {
                throw new ArgumentOutOfRangeException("Report due date cannot be after the end date");
            }

            if (endDate < term.PlanDueDate)
            {
                throw new ArgumentOutOfRangeException("Plan due date cannot be after the end date");
            }

            await _termRepository.CreateTerm(term);
        }

        public async Task UpdateTerm(Term term)
        {
            var existingTerm = await _termRepository.GetTermByIdAsync(term.Id) ?? throw new ArgumentNullException();

            var status = existingTerm.Status;
            if (status == TermStatus.New)
            {
                existingTerm.TermName = term.TermName;
                existingTerm.CreatorId = term.CreatorId;
                existingTerm.Duration = term.Duration;
                existingTerm.StartDate = term.StartDate;
                existingTerm.PlanDueDate = term.PlanDueDate;
                existingTerm.ReportDueDate = term.ReportDueDate;
                await _termRepository.UpdateTerm(term);
            }
            else
            {
                throw new ArgumentException("Term cannot be updated as it is not in the new status");
            }
        }

        public async Task DeleteTerm(Guid id)
        {
            var termToDelete = await _termRepository.GetTermByIdAsync(id);
            if (termToDelete != null)
            {
                if (termToDelete.Status == TermStatus.New)
                {
                    await _termRepository.DeleteTerm(termToDelete);
                }
                else
                {
                    throw new ArgumentException("Term cannot be deleted as it is not in the new status");
                }
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        public async Task<IEnumerable<Term>> GetAllTerms()
        {
            return await _termRepository.GetAllTerms();
        }

        public async Task CloseDueTerms()
        {
            IEnumerable<Term> terms = await _termRepository.GetAllTerms();
            foreach (var term in terms)
            {
                var endDate = term.StartDate.AddMonths(term.Duration);
                if (endDate > DateTime.Now || term.Status == TermStatus.Closed)
                    continue;
                term.Status = TermStatus.Closed;
                await _termRepository.UpdateTerm(term);
               Log.Information("Closed term with ID {TermId}.", term.Id);
            }
        }

        public async Task CloseTerm(Guid termId)
        {
            var term = await _termRepository.GetTermByIdAsync(termId);
            if (term != null)
            {
                term.Status = TermStatus.Closed;
                await _termRepository.UpdateTerm(term);
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        public async Task<IEnumerable<Term>> GetStartedTerms(Guid departId)
        {
            IEnumerable<Term> terms = await _termRepository.GetAllTerms();
            List<Term> startedTerms = [];
            foreach (var term in terms)
            {
                if (term.Status == TermStatus.InProgress && term.Plans!.Count == 0)
                {
                    startedTerms.Add(term);
                }
            }
            return startedTerms;
        }

        public async Task<IEnumerable<Term>> GetTermsWithNoPlanByUserId(Guid userId)
        {
            var department =  _departmentRepository.GetDepartmentByUserId(userId);
            var terms = await _termRepository.GetAllTerms();
            List<Term> termsWithNoPlan = [];
            foreach (var term in terms)
            {
                var isPlanExist = await _planRepository.IsPlanExist(term.Id, department.Id);
                if (term.Status == TermStatus.InProgress && !isPlanExist)
                {
                    termsWithNoPlan.Add(term);
                }
            }
            return termsWithNoPlan;
        }

        public async Task<IEnumerable<Term>> GetTermsWithUnFullFilledReports(Guid userId)
        {
            var department =  _departmentRepository.GetDepartmentByUserId(userId);
            var terms = await _termRepository.GetAllTerms();
            List<Term> result = [];
            foreach (var term in terms)
            {
                var duration = term.Duration;
                if (term.Status != TermStatus.InProgress){
                    continue;
                }
                foreach (var no in Enumerable.Range(0, duration))
                {
                    var month = term.StartDate.AddMonths(no).Month + '_' + term.StartDate.AddMonths(no).Year;
                    if (!await _reportRepository.IsReportExist(term.Id, department.Id, month.ToString()))
                    {
                       result.Add(term);
                       break;
                    }
                }
            }
            return result;
        }
    }
}
