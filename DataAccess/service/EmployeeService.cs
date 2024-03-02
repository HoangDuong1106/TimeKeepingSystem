using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        // Implement the GetAllAsync method from IEmployeeService by calling the corresponding repository method
        public async Task<List<EmployeeDTO>> GetAllAsync(Guid? roleId, Guid? DepartmentID, string? Searchname)
        {
            return await _employeeRepository.GetAllAsync(roleId, DepartmentID,Searchname);
        }

        public async Task<bool> AddAsync(EmployeeDTO a)
        {
            return await _employeeRepository.AddAsync(a);
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            return await _employeeRepository.SoftDeleteAsync(id);
        }

        public async Task<object> CreateEmployee(EmployeeDTO newEmployeeDTO)
        {
            return await _employeeRepository.CreateEmployee(newEmployeeDTO);
        }

        public async Task<EmployeeDTO> GetById(Guid employeeId)
        {
            return await _employeeRepository.GetById(employeeId);
        }

        public async Task<object> EditEmployee(EmployeeDTO employeeDTO)
        {
            return await _employeeRepository.EditEmployee(employeeDTO);
        }
    }
}