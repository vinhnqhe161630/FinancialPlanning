using FinancialPlanning.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanning.Data.Repositories
{
    public interface IPositionRepository
    {
        public Task<List<Position>> GetAllPositions();
    }
}
