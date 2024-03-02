namespace BusinessObject.DTO
{
    public class RequestDTO
    {
        public Guid Id { get; set; }
        public Guid? RequestLeaveId { get; set; }
        public Guid? RequestWorkTimeId { get; set; }
        public Guid EmployeeSendRequestId { get; set; }
        public string PathAttachmentFile { get; set; }
        public DateTime SubmitedDate { get; set; }
        public string Message { get; set; }
        public string Reason { get; set; }
        public Guid? RequestOverTimeId { get; set; }
        public Guid WorkslotEmployeeLeaveId { get; set; }
        public RequestStatus Status { get; set; }
        public bool IsDeleted { get; set; }
    }
}