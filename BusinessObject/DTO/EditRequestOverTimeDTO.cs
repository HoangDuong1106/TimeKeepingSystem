using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO
{
    public class EditRequestOverTimeDTO
    {
        public Guid? requestId { get; set; }
        public string? Name { get; set; }
        public string? Date { get; set; }
        public string? timeStart { get; set; }
        public string? timeEnd { get; set; }
        public string? reason { get; set; }
        public string? linkFile { get; set; }
        public string? messageFromDecider { get; set; }
        public Guid? workingStatusId { get; set; }
        public bool? isDeleted { get; set; }
        public int? status { get; set; }
    }
}
