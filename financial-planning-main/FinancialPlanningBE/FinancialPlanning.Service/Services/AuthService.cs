using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinancialPlanning.Data.Repositories;
using FinancialPlanning.Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using FinancialPlanning.Service.DTOs;
using FinancialPlanning.Common;

namespace FinancialPlanning.Service.Services
{
    public class AuthService
    {
        private readonly EmailService _emailService;
        private readonly IAuthRepository _authRepository;
        private readonly IDepartmentRepository _depRepository;

        private readonly IConfiguration _configuration;
        public AuthService(IAuthRepository authRepository, IConfiguration configuration, EmailService emailService, IDepartmentRepository depRepository)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _emailService = emailService;
            _depRepository = depRepository;
        }

        private async Task<string> ValidateToken(string token)
        {
            var jwtService = new JwtService(_configuration["JWT:Secret"]!, _configuration["JWT:ValidIssuer"]!);

            if (JwtService.IsTokenExpired(token))
            {
                throw new Exception("Token expired");
            }
            var principal = jwtService.GetPrincipal(token) ?? throw new Exception("Invalid token");
            var emailClaim = principal.FindFirst(ClaimTypes.Email) ?? throw new Exception("Email claim not found in token");

            var email = emailClaim.Value;

            var tokenFromDb = await _authRepository.GetToken(email) ?? throw new Exception("Token not found");

            // var tokenFromDb = await authRepository.GetToken(email) ?? throw new Exception("Token not found");
            if (tokenFromDb != token)
            {
                throw new Exception("Invalid token");
            }

            return email;
        }

        public async Task ResetPassword(User user)
        {
            // Validate token
            var token = user.Token ?? throw new Exception("Token not found");
            try
            {
                var email = await ValidateToken(token);
                user.Email = email;
                // If token is valid, proceed with password reset
                await _authRepository.ResetPassword(user);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GenerateToken(string email)
        {
            var jwtService = new JwtService(_configuration["JWT:Secret"]!, _configuration["JWT:ValidIssuer"]!);
            var token = jwtService.GenerateToken(email);
            _authRepository.SetToken(email, token);
            return token;
        }

        public void SendResetPasswordEmail(string userEmail, string resetToken)
        {
            var resetUrl = $"http://localhost:4200/reset-password?token={resetToken}";

            var email = new EmailDto
            {
                To = userEmail,
                Subject = "Password Reset",
                Body = $"We have just received a password reset request for {userEmail}.<br><br>" +
                $"Please click <a href=\"{resetUrl}\">here</a> to reset your password.<br><br>" +
                $"For your security, the link will expire in 24 hours or immediately after you reset your password."
            };

            _emailService.SendEmail(email);
        }

        public async Task<bool> IsUser(string email)
        {
            return await _authRepository.IsUser(email);
        }

        public async Task<string> LoginAsync(User userMapper)
        {

            //Check email and pass
            var user = await _authRepository.IsValidUser(userMapper.Email, userMapper.Password);

            if (user == null) return string.Empty;
            if (user.Status == UserStatus.Inactive) return "Inactive";
            //add email to claim
            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Email, user.Email),
                new("userId", user.Id.ToString()),
                new("username",user.Username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            //Get role of user
            var userRole = await _authRepository.GetRoleUser(user.Email);
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));

            //Add departmentName claim
            var departmentname = await _depRepository.GetDepartmentNameByUser(user);
            authClaims.Add(new Claim("departmentName", departmentname));

            var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

            //Create token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["JWT:ValidIssuer"],
                Audience = _configuration["JWT:ValidAudience"],
                Expires = DateTime.UtcNow.AddDays(30),
                Subject = new ClaimsIdentity(authClaims),
                SigningCredentials = new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha256)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);

        }

    }
}
