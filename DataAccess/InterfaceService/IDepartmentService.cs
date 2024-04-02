using BusinessObject.DTO;
using System;

namespace DataAccess.InterfaceService
{
    public interface IDepartmentService
    {
        Task<Team> GetDepartmentAsync(Guid departmentId);
        List<DepartmentDTO> GetDepartmentsWithoutManager();

        // Empty interface
        Task<List<EmployeeDTO>> GetEmployeesByDepartmentIdAsync(Guid departmentId);
    }
}
