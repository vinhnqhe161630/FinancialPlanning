using Microsoft.AspNetCore.Mvc;
using FinancialPlanning.Data.Entities;
using AutoMapper;
using FinancialPlanning.Service.Services;
using FinancialPlanning.WebAPI.Models.User;
using Microsoft.AspNetCore.Http;

namespace FinancialPlanning.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IMapper _mapper;

        public AuthController(AuthService authService, IMapper mapper)
        {
            _authService = authService;
            _mapper = mapper;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LogIn(LoginModel model)
        {
            IActionResult response;

            //InValid Model
            if (!ModelState.IsValid)
            {
                response = BadRequest();
            }
            //mapper loginmodel to user
            var user = _mapper.Map<User>(model);

            //Check acc and create token
            var token = await _authService.LoginAsync(user);

            //Invalid account and returned emtry
            if (string.IsNullOrEmpty(token))
            {
                response = Unauthorized(new { message = "Either email address or password is incorrect. Please try again" });
            }
            else if (token.Equals("Inactive"))
            {
                response = Unauthorized(new { message = "Your account is disabled. Contact us for help." });
            }
            else
            {
                response = Ok(new { token });
            }

            return Ok(response);
        }


        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            IActionResult response;

            var isUser = await _authService.IsUser(email);

            if (!isUser)
            {
             response = NotFound(new { message = "Email not found" });
            }
            else
            {

            var token = _authService.GenerateToken(email);

            _authService.SendResetPasswordEmail(email, token);
                response = Ok(new { message = "Email sent" });
            }

            return Ok(response);

        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid password reset request" });

                // Check for null values in model properties
                if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Token))
                    return BadRequest(new { message = "Password and token cannot be empty" });

                User user = _mapper.Map<User>(model);
                // Attempt to reset password
                await _authService.ResetPassword(user);

                return Ok(new { message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                // Handle potential errors
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
