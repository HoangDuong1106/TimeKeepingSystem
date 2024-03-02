using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class RiskPerformanceSetting
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public int Hours { get; set; }

    [Required]
    public int Days { get; set; }

    [Required]
    public DateTime DateSet { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}