using FinancialPlanning.Data.Data;
using FinancialPlanning.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using FinancialPlanning.Common;
using System.Text.RegularExpressions;

namespace FinancialPlanning.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }


        //Get list user
        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users!
                .Include(u => u.Department)
                .Include(u => u.Position)
                .Include(u => u.Role).ToListAsync();
        }

        // Update user
        public async Task UpdateUser(Guid id, User user)
        {
            var updateUser = await _context.Users!.FindAsync(user.Id) ?? throw new Exception("User not found");

            updateUser.FullName = user.FullName;
            updateUser.Email = user.Email;
            updateUser.PhoneNumber = Regex.Replace(user.PhoneNumber, @"\s+", " ").Trim();
            updateUser.DOB = user.DOB;
            updateUser.Address = Regex.Replace(user.Address, @"\s+", " ").Trim();
            updateUser.DepartmentId = user.DepartmentId;
            updateUser.PositionId = user.PositionId;
            updateUser.RoleId = user.RoleId;
            updateUser.Notes = Regex.Replace(user.Notes, @"\s+", " ").Trim();
            await _context.SaveChangesAsync();
        }

        //Add new User
        public async Task<(string username, string password)> AddNewUser(User user)
        {
            var existingUser = _context.Users!.FirstOrDefault(u => u.Email == user.Email);
            if (existingUser != null)
            {
                throw new Exception("Email already exists.");
            }

            var plainPassword = GeneratePassword();
            var createdUser = GenerateUserName(user.FullName);


            var newUser = new User
            {
                Username = GenerateUserName(user.FullName),
                Password = BCrypt.Net.BCrypt.HashPassword(plainPassword),
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = Regex.Replace(user.PhoneNumber, @"\s+", " ").Trim(),
                DOB = user.DOB,
                Address = Regex.Replace(user.Address, @"\s+", " ").Trim(),
                DepartmentId = user.DepartmentId,
                PositionId = user.PositionId,
                RoleId = user.RoleId,
                Status = user.Status,
                Notes = string.IsNullOrWhiteSpace(user.Notes) ? "N/A" : Regex.Replace(user.Notes, @"\s+", " ").Trim()
            };


            await _context.Users!.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return (newUser.Username, plainPassword);

        }

        //Auto GenerateUserName
        private string GenerateUserName(string fullName)
        {
            var nameParts = fullName.Split(' ');

            // Extract last name
            var lastName = nameParts[^1].ToUpper();

            // Extract initials
            var initials = string.Empty;
            for (var i = 0; i < nameParts.Length - 1; i++)
            {
                initials += nameParts[i][0].ToString().ToUpper();
            }

            // Combine initials and last name
            var userName = $"{char.ToUpper(lastName[0])}{lastName.Substring(1).ToLower()}{initials}";

            // Check username existence
            if (_context.Users == null) throw new Exception(string.Empty);
            var existingUserNames = _context.Users.Where(u => u.Username.StartsWith(userName)).Select(u => u.Username)
                .ToList();
            var count = 1;
            var finalUserName = userName;
            while (existingUserNames.Contains(finalUserName))
            {
                finalUserName = $"{userName}{count}";
                count++;
            }

            return finalUserName;
        }

        // Auto generate password
        private static string GeneratePassword(int length = 8)
        {
            var random = new Random();
            var password = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                password.Append(Constants.PasswordChars[random.Next(Constants.PasswordChars.Length)]);
            }

            return password.ToString();
        }

        //EncryptPassword
        private static string EncryptPassword()
        {
            var plainPassword = GeneratePassword();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            return hashedPassword;
        }

        //Get user by Id
        public async Task<User> GetUserById(Guid id)
        {
            var user = await _context.Users!
                .Include(u => u.Department)
                .Include(u => u.Position)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            // Thực hiện kiểm tra null ở đây nếu cần thiết

            if (user == null)
            {
                // Xử lý trường hợp không tìm thấy người dùng
                throw new Exception("User not found.");
            }

            return user;
        }


        //Update user status
        public async Task UpdateUserStatus(Guid id, UserStatus status)
        {
            var user = await _context.Users!.FindAsync(id);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            user.Status = status;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUser(Guid id)
        {
            var user = await _context.Users!.FindAsync(id);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            _context.Users!.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}