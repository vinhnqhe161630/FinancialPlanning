using Castle.Core.Configuration;
using FinancialPlanning.Data.Entities;
using FinancialPlanning.Data.Repositories;
using FinancialPlanning.Service.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.UnitTesting.Service.Services
{
    public class ReportServiceTest { }
    //{
    //    [Fact]
    //    public async Task GetReportsByEmail_ReturnsCorrectReports()
    //    {
    //        // Arrange
    //        var reportRepositoryMock = new Mock<IReportRepository>();
    //        var authRepositoryMock = new Mock<IAuthRepository>();
    //        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
         

    //        var service = new ReportService(reportRepositoryMock.Object,
    //            authRepositoryMock.Object, departmentRepositoryMock.Object);

    //        var email = "financialstaff@example.com";
    //        var role = "FinancialStaff";
    //        var departmentId = Guid.NewGuid();
    //        var reports = new List<Report> { new Report(), new Report() };

    //        authRepositoryMock.Setup(repo => repo.GetRoleUser(email)).ReturnsAsync(role);
    //        departmentRepositoryMock.Setup(repo => repo.GetDepartmentIdByEmail(email)).ReturnsAsync(departmentId.ToString());
    //        reportRepositoryMock.Setup(repo => repo.GetReportsByDepartId(departmentId)).ReturnsAsync(reports);

    //        // Act
    //        var result = await service.GetReportsByEmail(email);

    //        // Assert
    //        Assert.Equal(reports, result);
    //    }

    //    [Fact]
    //    public async Task GetReportsByEmail_ReturnsAllReports_ForNonFinancialStaff()
    //    {
    //        // Arrange
    //        var reportRepositoryMock = new Mock<IReportRepository>();
    //        var authRepositoryMock = new Mock<IAuthRepository>();
    //        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
           
    //        var service = new ReportService(reportRepositoryMock.Object, authRepositoryMock.Object, departmentRepositoryMock.Object);

    //        var email = "acc@example.com";
    //        var role = "Accountant";
    //        var reports = new List<Report> { new Report(), new Report() };

    //        authRepositoryMock.Setup(repo => repo.GetRoleUser(email)).ReturnsAsync(role);
    //        reportRepositoryMock.Setup(repo => repo.GetAllReports()).ReturnsAsync(reports);

    //        // Act
    //        var result = await service.GetReportsByEmail(email);

    //        // Assert
    //        Assert.Equal(reports, result);
    //    }
    
}
