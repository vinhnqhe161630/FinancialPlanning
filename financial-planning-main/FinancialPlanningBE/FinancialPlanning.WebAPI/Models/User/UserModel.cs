namespace FinancialPlanning.WebAPI.Models.User
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string DOB { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = null!;
        public string PositionName { get; set; } = null!;
        public Guid DepartmentId { get; set; }
        public Guid PositionId { get; set; }
        public string RoleName { get; set; } = null!;
        public Guid RoleId { get; set; }
        public int Status { get; set; }
        public string Notes { get; set; } = string.Empty;

    }
}
