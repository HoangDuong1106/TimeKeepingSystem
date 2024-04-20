using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTO
{
    public class RequestOverTimeDTO
    {
        public string? employeeNumber;

        public Guid? id { get; set; }
        public Guid? employeeId { get; set; }
        public string? employeeName { get; set; }
        public Guid? RequestOverTimeId { get; set; }
        public string? Name { get; set; }
        public string? Date { get; set; }
        public string? timeStart { get; set; }
        public string? timeEnd { get; set; }
        public double? NumberOfHour { get; set; }
        public bool? IsDeleted { get; set; }
        public string? reason { get; set; }
        public string? linkFile { get; set; }
        public string? statusRequest { get; set; }
        public string? status { get; set; }
        public double? timeInMonth { get; set; }
        public double? timeInYear { get; set; }
        public Guid? workingStatusId { get; set; }
        public string? submitDate { get; set; }
        public string? workingStatus { get; set; }
        public string? reasonReject { get; set; }
        public Guid? deciderId { get; set; }
    }
}