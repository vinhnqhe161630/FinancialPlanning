using FinancialPlanning.Data.Data;
using FinancialPlanning.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanning.Data.Repositories
{
    public class PositionRepository : IPositionRepository
    {
        private readonly DataContext _context;

        public PositionRepository(DataContext context)
        {
            this._context = context;

        }
        public async Task<List<Position>> GetAllPositions()
        {
            return await _context.Positions!.ToListAsync();
        }
    }
}
