namespace BusinessObject.DTO
{
    public class WorkTrackSettingDTO
    {
        public Guid Id { get; set; }
        public Guid? WorkTimeId { get; set; }
        public Guid? WorkDateId { get; set; }
        public Guid? WorkPermissionId { get; set; }
        public Guid? RiskPerfomanceId { get; set; }
        public bool IsDeleted { get; set; }
    }
}