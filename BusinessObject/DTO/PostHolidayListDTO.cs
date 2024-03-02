using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTO
{
    public class PostHolidayListDTO
    {
        public Guid HolidayId { get; set; }


        public Guid[] DepartmentIds { get; set; }
        public Guid DepartmentId { get; set; }

        public string? StartDate { get; set; }
        public string? EndDate { get; set; }

        public string HolidayName { get; set; }

        public string Description { get; set; }

        public bool IsRecurring { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}