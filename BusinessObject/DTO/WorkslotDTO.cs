using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.DTO
{
    public class WorkslotDTO
    {
        public Guid Id { get; set; }
        public Guid WorkTrackId { get; set; }
        public Guid? RequestLeaveId { get; set; }
        public Guid? RequestWorkTimeId { get; set; }
        public bool IsDeleted { get; set; }
    }
}