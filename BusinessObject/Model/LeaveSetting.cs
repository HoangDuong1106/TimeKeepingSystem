using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

public class LeaveSetting
{
    [Key]
    public Guid LeaveSettingId { get; set; }

    [Required]
    public string MaxDateLeave { get; set; }

    [Required]
    public bool IsManagerAssigned { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}

