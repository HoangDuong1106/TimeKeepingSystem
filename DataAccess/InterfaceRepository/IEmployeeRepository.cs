using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { public interface IEmployeeRepository { Task<bool> AddAsync(EmployeeDTO a); Task<object> CreateEmployee(EmployeeDTO newEmployeeDTO); Task<object> EditEmployee(EmployeeDTO employeeDTO); Task<List<EmployeeDTO>> GetAllAsync(Guid? roleId, Guid? DepartmentID, string? Searchname); Task<EmployeeDTO> GetById(Guid employeeId); Task<bool> SoftDeleteAsync(Guid id); } }