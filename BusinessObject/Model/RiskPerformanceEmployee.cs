using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class RiskPerformanceEmployee
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey("Employee")]
    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; }

    [Required]
    [ForeignKey("RiskPerformanceSetting")]
    public Guid RiskPerformanceSettingId { get; set; }
    public virtual RiskPerformanceSetting RiskPerformanceSetting { get; set; }

    [Required]
    [MaxLength]
    public string ViolationJSON { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}
