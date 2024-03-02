using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

public class Department
{
    [Key]
    public Guid Id { get; set; }

    [AllowNull]
    public Guid? ManagerId { get; set; }

    [Required]
    public string Name { get; set; }

    [ForeignKey("WorkTrackSetting")]
    public Guid WorkTrackId { get; set; }
    public WorkTrackSetting WorkTrackSetting { get; set; }

    public bool IsDeleted { get; set; } = false;  // Soft delete flag

    // New navigation property
    public ICollection<Employee> Employees { get; set; }
}
