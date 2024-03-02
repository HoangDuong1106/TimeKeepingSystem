namespace BusinessObject.DTO
{
    public class WorkslotEmployeeDTO
    {
        public Guid? workslotEmployeeId { get; set; }
        public DateTime? Date { get; set; }
        public string? CheckInTime { get; set; }
        public string? CheckOutTime { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? WorkslotId { get; set; }
        public Guid? AttendanceStatusId { get; set; }
        public bool IsDeleted { get; set; }
        public double? TimeLeaveEarly { get; set; }
        public double? TimeComeLate { get; set; }
        public string? SlotStart { get; set; }
        public string? SlotEnd { get; set; }
        public string? statusName { get; set; }
        public string? reason { get; set; }
        public string? linkFile { get; set; }
        public Guid? RequestId { get; set; }
    }
}