using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO
{
    public class RequestReasonDTO
    {
        public Guid requestId { get; set; }
        public string messageFromDecider { get; set; }
        public Guid? employeeIdDecider { get; set; }
    }
}
