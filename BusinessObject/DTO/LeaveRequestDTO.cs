using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BusinessObject.DTO
{
    public class LeaveRequestDTO
    {
        
        public Guid? id { get; set; }
        public string? Name { get; set; }
        public Guid? employeeId { get; set; }
        public string? employeeName { get; set; }
        public string? submitDate { get; set; }
        public string? startDate { get; set; }
        public string? endDate { get; set; }
        public string? leaveType { get; set; }
        public Guid? leaveTypeId { get; set; }
        public int? status { get; set; }
        public string? statusName { get; set; }
        public string? reason { get; set; }
        public string? reasonReject { get; set; }
        public string? linkFile { get; set; }
        public double? numberOfLeaveDate { get; set; }
        public List<DateRangeDTO?>? dateRange { get; set; }
    }
}
