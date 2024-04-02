using BusinessObject.DTO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

public class Employee
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; }

    [Required]
    [StringLength(50)]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public bool Gender { get; set; }

    [AllowNull]
    public string? Address { get; set; }

    [Required]
    [StringLength(50)]
    public string Role { get; set; }

    [AllowNull]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    public string EmploymentType { get; set; } = "FullTime"; // Add EmploymentType property

    [Required]
    public EmployeeStatus EmployeeStatus { get; set; }

    [ForeignKey("Team")]
    [AllowNull]
    public Guid? DepartmentId { get; set; }
    public Team? Department { get; set; }

    // Existing properties remain unchanged
    [ForeignKey("UserAccount")]
    public Guid UserID { get; set; } // Nullable if not all Employees will have UserAccounts
    public virtual UserAccount UserAccount { get; set; } // Navigation Property

    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}
