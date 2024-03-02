namespace BusinessObject.DTO
{
    public class LeaveTypeDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int? AllowedDays { get; set; }
        public LeaveCycle LeaveCycle { get; set; }
        public bool CanCarryForward { get; set; }
        public int TotalBalance { get; set; }
        public Guid LeaveSettingId { get; set; }
        public bool IsDeleted { get; set; }
    }
}