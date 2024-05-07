using AutoMapper;
using FinancialPlanning.Data.Entities;
using FinancialPlanning.Data.Repositories;
using FinancialPlanning.Service.Services;
using FinancialPlanning.WebAPI.Models.Department;
using FinancialPlanning.WebAPI.Models.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.Text.Json;
using FinancialPlanning.WebAPI.Models.Report;
using FinancialPlanning.WebAPI.Models.Term;
using FinancialPlanning.WebAPI.Models.Role;
using FinancialPlanning.WebAPI.Models.Position;
using FinancialPlanning.Common;
using Microsoft.AspNetCore.Authorization;

namespace FinancialPlanning.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IMapper mapper, UserService userService) : ControllerBase
    {
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly UserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        //Get all users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsers();
                var userListModels = users.Select(u => _mapper.Map<UserModel>(u)).ToList();

                return Ok(userListModels);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //Get user by id
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetUserById(id);
            var userModel = _mapper.Map<UserModel>(user);
            return userModel == null ? NotFound() : Ok(userModel);
        }

        //Add new user
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddNewUser(AddUserModel userModel)
        {

            try
            {
                var user = _mapper.Map<User>(userModel);
                await _userService.AddNewUser(user);
                return Ok(user);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Email already exists"))
                {
                    return BadRequest("Email already exists");
                }
                else
                {
                    // Xử lý các loại lỗi khác nếu cần thiết
                    return StatusCode(500, "An error occurred while processing your request");
                }
            }
        }

        // Update User
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(Guid id, AddUserModel userModel)
        {
            var user = _mapper.Map<User>(userModel);
            user.Id = id;
            await _userService.UpdateUser(id, user);
            return Ok(new { message = $"User with id {id} updated successfully!" });
        }
        // Update status User
        [HttpPut("{id:guid}/{status:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserStatus(Guid id, UserStatus status)
        {
            try
            {
                await _userService.UpdateUserStatus(id, status);
                return Ok(new { message = $"User with id {id} updated successfully!" });
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error updating user with id {id}: {ex.Message}" });
            }
        }
        //Delete user
        [HttpDelete("{id}/delete")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                await _userService.DeleteUser(id);
                return Ok(new { message = $"User with id {id} removed successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error delete user with id {id}: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("AllDepartments")]
        public async Task<ActionResult<List<Department>>> GetAllDepartments()
        {
            try
            {
                var departments = await _userService.GetAllDepartment();
                var departmentViewModel = _mapper.Map<List<DepartmentViewModel>>(departments);
                return Ok(departmentViewModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet]
        [Route("AllRoles")]
        public async Task<ActionResult<List<Department>>> GetAllRoles()
        {
            try
            {
                var roles = await _userService.GetAllRoles();
                var roleViewModel = _mapper.Map<List<RoleViewModel>>(roles);
                return Ok(roleViewModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet]
        [Route("AllPositions")]
        public async Task<ActionResult<List<Department>>> GetAllPositions()
        {
            try
            {
                var positions = await _userService.GetAllPositions();
                var positionViewModel = _mapper.Map<List<PositionViewModel>>(positions);
                return Ok(positionViewModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }


}
