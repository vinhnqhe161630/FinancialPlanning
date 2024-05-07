using FinancialPlanning.Common;
using FinancialPlanning.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FinancialPlanning.Data.Repositories
{
    public interface IUserRepository
    {
        public Task<(string username, string password)> AddNewUser(User user);
        public Task UpdateUser(Guid id, User user);
        public Task DeleteUser(Guid id);
        public Task<List<User>> GetAllUsers();
        public Task<User> GetUserById(Guid id);
        public Task UpdateUserStatus(Guid id, UserStatus status);
    }
}
