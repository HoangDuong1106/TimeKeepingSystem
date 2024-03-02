using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class WorkslotEmployee
{
    [Key]
    public Guid Id { get; set; }

    public string? CheckInTime { get; set; }

    public string? CheckOutTime { get; set; }

    [Required]
    [ForeignKey("Employee")]
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; }

    [Required]
    [ForeignKey("Workslot")]
    public Guid WorkslotId { get; set; }
    public Workslot Workslot { get; set; }

    [Required]
    [ForeignKey("AttendanceStatus")]
    public Guid AttendanceStatusId { get; set; }
    public AttendanceStatus AttendanceStatus { get; set; }

    public bool IsDeleted { get; set; }
}
