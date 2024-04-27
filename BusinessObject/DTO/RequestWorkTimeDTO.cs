using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTO
{
    public class RequestWorkTimeDTO
    {
        public string? employeeNumber { get; set; }

        public Guid? Id { get; set; }
        public string? employeeName { get; set; }
        public Guid? employeeId { get; set; }
        public string? Name { get; set; }
        public string? RealHourStart { get; set; }
        public string? RealHourEnd { get; set; }
        public string? NumberOfComeLateHour { get; set; }
        public string? NumberOfLeaveEarlyHour { get; set; }
        public Guid WorkslotEmployeeId { get; set; }
        public Guid workslotEmployeeMorningId { get; set; }
        public string? SlotStart { get; set; }
        public string? SlotEnd { get; set; }
        public double? TimeInMonth { get; set; }
        public double? TimeInYear { get; set; }
        public string? DateOfWorkTime { get; set; }
        public bool IsDeleted { get; set; }
        public int status { get; set; }
        public string? statusName { get; set; }
        public string? reason { get; set; }
        public string? linkFile { get; set; }
        public string? submitDate { get; set; }
        public string? reasonReject { get; set; }
        public Guid? deciderId { get; set; }
        public string? CheckInTime { get; set; }
        public string? CheckOutTime { get; set; }
    }
}