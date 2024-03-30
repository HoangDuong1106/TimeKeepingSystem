using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO
{
    public class CombinedRequestDTO
    {
        public List<RequestOverTimeDTO> OverTimeRequests { get; set; } = new List<RequestOverTimeDTO>();
        public List<RequestWorkTimeDTO> WorkTimeRequests { get; set; } = new List<RequestWorkTimeDTO>();
        public List<LeaveRequestDTO> LeaveRequests { get; set; } = new List<LeaveRequestDTO>();
    }
}
