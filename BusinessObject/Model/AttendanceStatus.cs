using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

public class AttendanceStatus
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey("LeaveType")]
    [AllowNull]
    public Guid? LeaveTypeId { get; set; }
    public LeaveType? LeaveType { get; set; }

    [ForeignKey("WorkingStatus")]
    [AllowNull]
    public Guid? WorkingStatusId { get; set; }
    public WorkingStatus? WorkingStatus { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}
