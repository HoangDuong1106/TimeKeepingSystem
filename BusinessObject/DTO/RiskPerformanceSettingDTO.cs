namespace BusinessObject.DTO
{
    public class RiskPerformanceSettingDTO
    {
        public Guid Id { get; set; }
        public int Hours { get; set; }
        public int Days { get; set; }
        public DateTime DateSet { get; set; }
        public bool IsDeleted { get; set; }
    }
}