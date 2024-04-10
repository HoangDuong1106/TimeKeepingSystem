using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

public class RequestLeave
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    //[Required]
    //[ForeignKey("LeaveType")]
    //public Guid LeaveTypeId { get; set; }
    //public LeaveType LeaveType { get; set; }

    [Required]
    //[ForeignKey("LeaveType")]
    public Guid LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public virtual List<WorkslotEmployee> WorkslotEmployees { get; set; }
    [AllowNull]
    public Guid? SupportEmployeeId { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete flag

}
