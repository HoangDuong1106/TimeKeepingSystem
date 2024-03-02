using System.ComponentModel.DataAnnotations;

public class WorkPermissionSetting
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength]
    public string Name { get; set; }
    public bool IsDeleted { get; set; } = false;
}
