using FinancialPlanning.Common;
using FinancialPlanning.Data.Data;
using FinancialPlanning.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanning.Data.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;

        public AuthRepository(DataContext context)
        {
            this._context = context;
        }

        public async Task<User?> IsValidUser(string email, string password)
        {
            var user = await _context.Users!.SingleOrDefaultAsync(u =>
                u.Email == email);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return user;
            }

            return null;
        }

        public async Task<string> GetRoleUser(string email)
        {
            if (_context.Users == null)
            {
                return string.Empty;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return string.Empty;
            }

            var role = await _context.Roles!.FirstOrDefaultAsync(r => r.Id == user.RoleId);
            return role != null ? role.RoleName : string.Empty;
        }

        public async Task ResetPassword(User user)
        {
            var userToUpdate = await _context.Users!.SingleOrDefaultAsync(u => u.Email == user.Email) ??
                               throw new Exception("User not found");
            userToUpdate.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            userToUpdate.Token = null;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsUser(string email)
        {
            var user = await _context.Users!.FirstOrDefaultAsync(u => u.Email == email && u.Status == UserStatus.Active);
            return user != null;
        }

        public async Task SetToken(string email, string token)
        {
            var user = await _context.Users!.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                user.Token = token;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> GetToken(string email)
        {
            var user = await _context.Users!.FirstOrDefaultAsync(u => u.Email == email) ??
                       throw new Exception("User not found");
            return user.Token!;
        }
    }
}