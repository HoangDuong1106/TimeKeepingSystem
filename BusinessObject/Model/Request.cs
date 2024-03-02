using BusinessObject.DTO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

public class Request
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey("RequestLeave")]
    [AllowNull]
    public Guid? RequestLeaveId { get; set; }
    public RequestLeave? RequestLeave { get; set; }

    [ForeignKey("RequestWorkTime")]
    [AllowNull]
    public Guid? RequestWorkTimeId { get; set; }
    public RequestWorkTime? RequestWorkTime { get; set; }

    [Required]
    [ForeignKey("Employee")]
    public Guid EmployeeSendRequestId { get; set; }
    public Employee EmployeeSendRequest { get; set; }

    [StringLength(1000)]
    public string Message { get; set; }
    [StringLength(1000)]
    public string PathAttachmentFile { get; set; }

    [StringLength(1000)]
    public string Reason { get; set; }

    [Required]
    public DateTime SubmitedDate { get; set; }

    [ForeignKey("RequestOverTime")]
    [AllowNull]
    public Guid? RequestOverTimeId { get; set; }
    public RequestOverTime? RequestOverTime { get; set; }
    public RequestType? requestType { get; set; }

    [StringLength(20)]
    public RequestStatus Status { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}
