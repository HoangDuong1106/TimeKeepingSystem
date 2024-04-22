using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO
{
    public class CreateWorkSlotRequest
    {
        public Guid? departmentId { get; set; }
        public string? month { get; set; }
        public Guid? employeeId { get; set; }  // Add this new field
    }
}
