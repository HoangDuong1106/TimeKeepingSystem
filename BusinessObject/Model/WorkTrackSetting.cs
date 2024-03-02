using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

public class WorkTrackSetting
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey("WorkTimeSetting")]
    [AllowNull]
    public Guid? WorkTimeId { get; set; }
    public WorkTimeSetting? WorkTimeSetting { get; set; }

    [ForeignKey("WorkDateSetting")]
    [AllowNull]
    public Guid? WorkDateId { get; set; }
    public WorkDateSetting? WorkDateSetting { get; set; }

    [ForeignKey("RiskPerfomanceSetting")]
    [AllowNull]
    public Guid? RiskPerfomanceId { get; set; }
    public RiskPerformanceSetting? RiskPerfomanceSetting { get; set; }

    [ForeignKey("LeaveSetting")]
    [AllowNull]
    public Guid? LeaveSettingId { get; set; }
    public LeaveSetting? LeaveSetting { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete flag
}
