using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO
{
    public class MaxDateLeavesDTO
    {
        public int Year { get; set; }
        public Dictionary<Guid, int> LeaveTypeMaxDays { get; set; } // Assuming Guid is the ID of the LeaveType
    }

}
