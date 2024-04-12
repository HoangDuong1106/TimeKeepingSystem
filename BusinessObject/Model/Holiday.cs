using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public class Holiday
{
    [Key]
    public Guid HolidayId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [MaxLength(255)]
    public string HolidayName { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    public bool IsRecurring { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}

