using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class UserAccount
{
    [Key]
    public Guid ID { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [Required]
    public string SaltPassword { get; set; }

    [Required]
    public bool IsActive { get; set; }

    [ForeignKey("Employee")]
    public Guid? EmployeeId { get; set; }

    [ForeignKey("RoleID")]
    public Guid RoleID { get; set; }
    public Role Role { get; set; }

    public virtual Employee Employee { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}
