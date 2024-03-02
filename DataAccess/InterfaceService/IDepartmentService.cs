using BusinessObject.DTO;
using System;

namespace DataAccess.InterfaceService
{
    public interface IDepartmentService
    {
        Task<Department> GetDepartmentAsync(Guid departmentId);
        List<DepartmentDTO> GetDepartmentsWithoutManager();

        // Empty interface
        Task<List<EmployeeDTO>> GetEmployeesByDepartmentIdAsync(Guid departmentId);
    }
}
