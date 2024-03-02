using BusinessObject.DTO;
using System;

namespace DataAccess.InterfaceService
{
    public interface IEmployeeService
    {
        // Empty interface
        Task<bool> AddAsync(EmployeeDTO a);
        Task<object> CreateEmployee(EmployeeDTO newEmployeeDTO);
        Task<object> EditEmployee(EmployeeDTO employeeDTO);
        Task<List<EmployeeDTO>> GetAllAsync(Guid? roleId, Guid? DepartmentID, string? Searchname);
        Task<EmployeeDTO> GetById(Guid employeeId);
        Task<bool> SoftDeleteAsync(Guid id);
    }
}
