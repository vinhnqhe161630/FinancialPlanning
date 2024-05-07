using FinancialPlanning.Data.Data;
using FinancialPlanning.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanning.Data.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly DataContext context;

        public DepartmentRepository(DataContext context)
        {
            this.context = context;

        }
        // Get list departments
        public async Task<List<Department>> GetAllDepartments()
        {
            return await context.Departments!.ToListAsync();
        }

        //Get DepartmentId by email
        public async Task<string> GetDepartmentIdByEmail(string email)
        {
            var user = await context.Users!.FirstOrDefaultAsync(u => u.Email == email);
            return user!.DepartmentId.ToString();
        }

        

        //Get name of department
        public async Task<string> GetDepartmentNameByUser(User user)
        {

            var department = await context.Departments!.FirstOrDefaultAsync(d => d.Id == user.DepartmentId);
            if (department != null)
            {
                return department.DepartmentName;
            }

            return string.Empty;
        }
        // Get all department
        public async Task<List<Department>> GetAllDepartment()
        {
            return await context.Departments!.ToListAsync();
        
        }

        // public Task<Department> GetDepartmentByUserName(string username)
        // {
        //     var department = (from user in context.Users
        //                       where user.Username == username
        //                       select user.Department).FirstOrDefault();
        //     return Task.FromResult(department)!;
        // }

        public Department GetDepartmentByUserName(string username)
        {
            var department = (from user in context.Users
                              where user.Username == username
                              select user.Department).FirstOrDefault();
            return department!;
        }

        public Guid GetDepartmentIdByUid(Guid id)
        {
            var department = context.Users!.FirstOrDefault(x => x.Id == id)!.DepartmentId;
            return department;
        }

        public Department GetDepartmentByUserId(Guid id)
        {
            var department = (from user in context.Users
                              where user.Id == id
                              select user.Department).FirstOrDefault();
            return department!;
        }
    }
}
