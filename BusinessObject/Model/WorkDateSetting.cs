using System.ComponentModel.DataAnnotations;

public class WorkDateSetting
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength]
    public string DateStatus { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}
