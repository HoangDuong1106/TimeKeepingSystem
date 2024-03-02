namespace BusinessObject.DTO
{
    public class LeaveSettingDTO
    {
        public Guid? LeaveSettingId { get; set; }
        public List<MaxDateLeaveDTO>? MaxDateLeaves { get; set; }
        public bool? IsManagerAssigned { get; set; }
        public bool? IsDeleted { get; set; }
    }
}