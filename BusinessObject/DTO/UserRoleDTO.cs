namespace BusinessObject.DTO
{
    public class UserRoleDTO
    {
        public Guid UserID { get; set; }
        public Guid RoleID { get; set; }
        public bool IsDeleted { get; set; }
    }
}