namespace BusinessObject.DTO
{
    public partial class ChangePasswordDTO
    {
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}