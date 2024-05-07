using FinancialPlanning.Service.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.UnitTesting.Service.Services
{
    public class TokenServiceTest
    {
       
        [Fact]
        public void GetEmailFromToken_ValidToken_ReturnsEmail()
        {
            // Arrange
            var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
             "eyJlbWFpbCI6ImZpYW5jaUBlbWFpbC5jb20iLCJ1c2VybmFtZSI6IkZpbmFuY2lhbFN0" +
             "YWZmMSIsImp0aSI6ImYyMWVlZjhiLTk3YTQtNDE4Ni04YzVlLWY3Y2Y1ZjFkNzQ2NSIs" +
             "InJvbGUiOiJGaW5hbmNpYWxTdGFmZiIsImRlcGFydG1lbnROYW1lIjoiQWNjb3VudGluZyIsI" +
             "m5iZiI6MTcxMDMxNDkzOSwiZXhwIjoxNzEwMzE2MTM5LCJpYXQiOjE3MTAzMTQ5MzksIml" +
             "zcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTA4NSIsImF1ZCI6IlVzZXIifQ.LagnZZ9RelLUkedi" +
             "kq1ESnDTIDr6RLt4F5yL-MtIq1E";
            var tokenService = new TokenService();

            // Act
            var email = tokenService.GetEmailFromToken(token);

            // Assert
            Assert.Equal("fianci@email.com", email);
        }
    }
}
