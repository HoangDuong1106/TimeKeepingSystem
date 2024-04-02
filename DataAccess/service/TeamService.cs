using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class TeamService : IDepartmentService
    {
        private readonly IDepartmentRepository _DepartmentRepository;

        public TeamService(IDepartmentRepository DepartmentRepository)
        {
            _DepartmentRepository = DepartmentRepository;
        }

        // Implement the GetAllAsync method from IDepartmentService by calling the corresponding repository method
        public async Task<List<DepartmentDTO>> GetAllAsync()
        {
            return await _DepartmentRepository.GetAllAsync();
        }

        public async Task<List<EmployeeDTO>> GetEmployeesByDepartmentIdAsync(Guid departmentId)
        {
            return await _DepartmentRepository.GetEmployeesByDepartmentIdAsync(departmentId);
        }

        public async Task<Team> GetDepartmentAsync(Guid departmentId)
        {
            return await _DepartmentRepository.GetDepartmentAsync(departmentId);
        }

        public List<DepartmentDTO> GetDepartmentsWithoutManager()
        {
            return _DepartmentRepository.GetDepartmentsWithoutManager();
        }
    }
}