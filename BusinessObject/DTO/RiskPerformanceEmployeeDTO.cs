namespace BusinessObject.DTO
{
    public class RiskPerformanceEmployeeDTO
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid RiskPerformanceSettingId { get; set; }
        public string ViolationJSON { get; set; }
        public bool IsDeleted { get; set; }
    }
}