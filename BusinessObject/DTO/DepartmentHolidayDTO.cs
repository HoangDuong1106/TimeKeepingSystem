namespace BusinessObject.DTO
{
    public class DepartmentHolidayDTO
    {
        public Guid? HolidayId { get; set; }
        public string? HolidayName { get; set; }
        public string? Description { get; set; }
        public bool? IsRecurring { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}