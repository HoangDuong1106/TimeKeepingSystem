    using BusinessObject.DTO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

public class LeaveType
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    public int? AllowedDays { get; set; } // Null for unlimited days

    [Required]
    public LeaveCycle LeaveCycle { get; set; }

    [Required]
    public bool CanCarryForward { get; set; }

    [Required]
    public int TotalBalance { get; set; }

    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}
