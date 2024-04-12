using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class DepartmentHolidayException
{
    [Key]
    public Guid ExceptionId { get; set; }

    [ForeignKey("Holiday")]
    public Guid HolidayId { get; set; }
    public Holiday DepartmentHoliday { get; set; }

    [Required]
    public DateTime ExceptionDate { get; set; }

    [Required]
    [MaxLength(255)]
    public string Reason { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}

