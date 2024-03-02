namespace BusinessObject.DTO
{
    public class RequestLeaveDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid LeaveTypeId { get; set; }
        public IEnumerable<Guid?>? WorkslotEmployeeIds { get; set; }
        public bool IsDeleted { get; set; }
    }
}