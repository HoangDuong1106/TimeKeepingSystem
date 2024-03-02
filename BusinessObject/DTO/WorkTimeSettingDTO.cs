namespace BusinessObject.DTO
{
    public class WorkTimeSettingDTO
    {
        public Guid Id { get; set; }
        public double ExpectWorkHour { get; set; }
        public double MinimumProductiveWorkHour { get; set; }
        public bool IsDeleted { get; set; }
    }
}