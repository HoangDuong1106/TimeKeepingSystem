namespace BusinessObject.DTO
{
    public class UserAccountDTO
    {
        public Guid ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; }
        public Guid? EmployeeId { get; set; }
        public bool IsDeleted { get; set; }
    }
}