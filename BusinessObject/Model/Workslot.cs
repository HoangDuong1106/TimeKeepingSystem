using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

public class Workslot
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsMorning { get; set; }
    public DateTime DateOfSlot { get; set; }
    public string? FromHour { get; set; }
    public string? ToHour { get; set; }

    [ForeignKey("Team")]
    [AllowNull]
    public Guid? DepartmentId { get; set; }
    public Team? Department { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}
