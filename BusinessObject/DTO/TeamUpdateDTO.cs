using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO
{
    public class TeamUpdateDTO
    {
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public List<TeamMemberUpdateDTO> Team { get; set; }
    }

    public class TeamMemberUpdateDTO
    {
        public Guid EmployeeId { get; set; }
        public string RoleName { get; set; }
    }

}
