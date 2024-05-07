using FinancialPlanning.Data.Entities;

namespace FinancialPlanning.Data.Repositories
{
    public interface IAuthRepository
    {
        public Task<User?> IsValidUser(string email, string password);

        public  Task<string> GetRoleUser(string email);
        public Task ResetPassword(User user);
        public Task<bool> IsUser(string email);
        public Task SetToken(string email, string token);
        public Task<string> GetToken(string email);

    }
}
