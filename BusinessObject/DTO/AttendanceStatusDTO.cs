namespace BusinessObject.DTO
{
    public class AttendanceStatusDTO
    {
        public Guid Id { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public Guid? WorkingStatusId { get; set; }
        public bool IsDeleted { get; set; }
        public string? Name { get; set; }

    }
}