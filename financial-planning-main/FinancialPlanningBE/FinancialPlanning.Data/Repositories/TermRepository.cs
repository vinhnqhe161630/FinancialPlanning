using Microsoft.EntityFrameworkCore;
using FinancialPlanning.Data.Entities;
using FinancialPlanning.Data.Data;

namespace FinancialPlanning.Data.Repositories
{
    public class TermRepository(DataContext context) : ITermRepository
    {
        private readonly DataContext _context = context;

        public async Task<Guid> CreateTerm(Term term)
        {
            term.Id = Guid.NewGuid();
            _context.Terms!.Add(term);
            await _context.SaveChangesAsync();
            return term.Id;
        }

        public async Task DeleteTerm(Term term)
        {
            _context.Terms!.Remove(term);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Term>> GetAllTerms()
        {
            return await _context.Terms!.
                Include(t => t.Plans).ToListAsync();
        }

        public async Task<Term?> GetTermByIdAsync(Guid id)
        {
             var term = await _context.Terms!.FindAsync(id);
            return term;

        }

        public Term GetTermById(Guid id)
        {
            return _context.Terms!.Find(id) ?? throw new Exception("Term not found");
        }

        public async Task UpdateTerm(Term term)
        {
            var existingTerm = await _context.Terms!.FindAsync(term.Id) ?? throw new Exception("Term not found");
            // Update the properties of the existing term entity with the values from the provided term object
            existingTerm.TermName = term.TermName;
            existingTerm.CreatorId = term.CreatorId;
            existingTerm.Duration = term.Duration;
            existingTerm.StartDate = term.StartDate;
            existingTerm.PlanDueDate = term.PlanDueDate;
            existingTerm.ReportDueDate = term.ReportDueDate;

            await _context.SaveChangesAsync();
        }

        //Get total term in the year
        public async Task<int> GetTotalTermByYear(int year)
        {
           var totalTerm = await _context.Terms.Where(t => t.StartDate.Year == year).CountAsync();
            return totalTerm;
        }





        // Implement your repository methods here

    }
}
