using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

public class RequestWorkTime
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [AllowNull]
    public string? RealHourStart { get; set; }
    [AllowNull]
    public string? RealHourEnd { get; set; }

    public DateTime? DateOfSlot { get; set; }
    [AllowNull]
    public float? NumberOfComeLateHour { get; set; }

    [AllowNull]
    public float? NumberOfLeaveEarlyHour { get; set; }

    [Required]
    [ForeignKey("WorkslotEmployee")]
    public Guid WorkslotEmployeeId { get; set; }
    public WorkslotEmployee WorkslotEmployee { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete flag

}
