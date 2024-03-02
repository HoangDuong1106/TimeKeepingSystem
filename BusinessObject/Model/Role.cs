using System.ComponentModel.DataAnnotations;

public class Role
{
    [Key]
    public Guid ID { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    public virtual ICollection<UserAccount>? UserRoles { get; set; } // Navigation Property for UserRole
    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}
