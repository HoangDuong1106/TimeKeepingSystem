using BusinessObject.DTO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class RequestOverTime
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }
    [Required]
    public DateTime DateOfOverTime { get; set; }
    [Required]
    public DateTime FromHour { get; set; }
    [Required]
    public double NumberOfHour { get; set; }
    [Required]
    public DateTime ToHour { get; set; }
    public string? CheckInTime { get; set; }
    public string? CheckOutTime { get; set; }

    [ForeignKey("WorkingStatus")]
    public Guid WorkingStatusId { get; set; }
    public WorkingStatus? WorkingStatus { get; set; }

    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}
