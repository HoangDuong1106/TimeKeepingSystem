namespace BusinessObject.DTO
{
    public class AccountDTO
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? Gender { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid? RoleID { get; set; }
        public Guid? DepartmentID { get; set; }
    }
}