using System.ComponentModel.DataAnnotations;

public class WorkTimeSetting
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string FromHourMorning { get; set; }

    [Required]
    public string ToHourMorning { get;set; }

    [Required]
    public string FromHourAfternoon { get; set; }

    [Required]
    public string ToHourAfternoon { get; set; }

    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}
