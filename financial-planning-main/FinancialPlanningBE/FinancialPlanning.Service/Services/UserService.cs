using FinancialPlanning.Common;
using FinancialPlanning.Data.Entities;
using FinancialPlanning.Data.Repositories;
using FinancialPlanning.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanning.Service.Services
{
    public class UserService
    {

        private readonly IUserRepository _userrepository;
        private readonly IDepartmentRepository _departmentrepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPositionRepository _positionRepository;
        private readonly EmailService _emailService;

        public UserService(IUserRepository userRepository, IDepartmentRepository departmentRepository
            , IRoleRepository roleRepository, IPositionRepository positionRepository, EmailService emailService)
        {

            _userrepository = userRepository;
            _departmentrepository = departmentRepository;
            _roleRepository = roleRepository;
            _positionRepository = positionRepository;
            _emailService = emailService;

        }
        //Get all user
        public async Task<List<User>> GetAllUsers()
        {
            var result = await _userrepository.GetAllUsers();
            return result;
        }

        //Get all department
        public async Task<List<Department>> GetAllDepartment()
        {
            return await _departmentrepository.GetAllDepartment();
        }
        //Get all position
        public async Task<List<Position>> GetAllPositions()
        {
            return await _positionRepository.GetAllPositions();
        }
        //Get all role
        public async Task<List<Role>> GetAllRoles()
        {
            return await _roleRepository.GetAllRoles();
        }
        //Get user by Id
        public async Task<User> GetUserById(Guid id)
        {
            var user = await _userrepository.GetUserById(id);
            return user;

        }
        //Update user
        public async Task UpdateUser(Guid id, User user)
        {
            await _userrepository.UpdateUser(id, user);
        }
        //Add new user
        public async Task AddNewUser(User user)
        {
            (string username, string password) = await _userrepository.AddNewUser(user);


            // Gửi email thông báo đã thêm mới người dùng
            var emailDto = new EmailDto
            {
                To = user.Email, // Địa chỉ email của người nhận
                Subject = $"no-reply-email-Financial Planning-system <{username}>",
                Body = $"This email is from Financial Planning system,<br>" +
            "Your account has been created. Please use the following credential to login:<br><br>" +
            $"- User name: <strong>{user.Email}</strong><br>" +
            $"- Password: <strong>{password}</strong><br><br>" +
            $"If anything wrong, please reach-out financialplanapp@gmail.com. We are so sorry for this inconvenience.<br>" +
            "Thanks & Regards!<br>" +
            "Financial Planning Team."
            };
            _emailService.SendEmail(emailDto);
        }

        //Update user status
        public async Task UpdateUserStatus(Guid id, UserStatus status)
        {
            await _userrepository.UpdateUserStatus(id, status);
        }
        //Delete user
        public async Task DeleteUser(Guid id)
        {
            await _userrepository.DeleteUser(id);
        }
    }
}
