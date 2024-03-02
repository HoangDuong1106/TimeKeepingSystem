namespace BusinessObject.DTO
{
    public class DepartmentHolidayExceptionDTO
    {
        public Guid ExceptionId { get; set; }
        public Guid HolidayId { get; set; }
        public DateTime ExceptionDate { get; set; }
        public string Reason { get; set; }
        public bool IsDeleted { get; set; }
    }
}