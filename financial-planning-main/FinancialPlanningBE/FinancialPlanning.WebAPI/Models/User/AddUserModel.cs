namespace FinancialPlanning.WebAPI.Models.User
{
    public class AddUserModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string DOB { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public Guid PositionId { get; set; }
        public Guid RoleId { get; set; }
        public int Status { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
