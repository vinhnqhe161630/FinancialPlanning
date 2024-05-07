using System.Collections.Generic;
using FinancialPlanning.Data.Entities;

namespace FinancialPlanning.Data.Repositories
{
    public interface ITermRepository
    {
        public Task<List<Term>> GetAllTerms();
        public Task<Term?> GetTermByIdAsync(Guid id);
        public Term GetTermById(Guid id);
        public Task<Guid> CreateTerm(Term term);
        public Task UpdateTerm(Term term);
        public Task DeleteTerm(Term term);
        public Task<int> GetTotalTermByYear(int year);
    }
}
