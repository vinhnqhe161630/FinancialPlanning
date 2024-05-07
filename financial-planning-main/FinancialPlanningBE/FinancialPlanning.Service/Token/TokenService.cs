using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanning.Service.Token
{
    public class TokenService
    {
        //Get email form token
        public string GetEmailFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            if (tokenHandler.ReadToken(token) is not JwtSecurityToken jwtToken) return string.Empty;
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");
            if (emailClaim != null)
            {
                return emailClaim.Value;
            }

            return string.Empty;
        }
       
    }
}
