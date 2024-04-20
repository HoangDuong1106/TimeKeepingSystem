namespace BusinessObject.DTO
{
    public class EmployeeDTO
    {
        public Guid? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? RoleInTeam { get; set; }
        public string? Address { get; set; }
        public bool? Gender { get; set; }
        public Guid? ManagerId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? EmployeeStatusName { get; set; }
        public Guid? UserID { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsDeleted { get; set; }
        public EmployeeRole? employeeRole { get; set; }
        public string? DepartmentName { get; set; }
        public bool? IsActive { get; set; }
        public int? EmployeeStatus { get; set; }
        public string? RoleName { get; set; }
        public Guid? RoleId { get; set; }
        public string? EmploymentType { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? DeviceSerialNumber { get; set; }
    }
}