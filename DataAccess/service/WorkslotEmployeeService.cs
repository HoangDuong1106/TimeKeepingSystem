using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class WorkslotEmployeeService : IWorkslotEmployeeService
    {
        private readonly IWorkslotEmployeeRepository _workslotEmployeeRepository;

        public WorkslotEmployeeService(IWorkslotEmployeeRepository workslotEmployeeRepository)
        {
            _workslotEmployeeRepository = workslotEmployeeRepository;
        }

        // Implement the GetAllAsync method from IWorkslotEmployeeService by calling the corresponding repository method
        public async Task<object> GenerateWorkSlotEmployee(CreateWorkSlotRequest request)
        {
            return await _workslotEmployeeRepository.GenerateWorkSlotEmployee(request);
        }

        public async Task<object> GetWorkSlotEmployeeByEmployeeId(Guid employeeId)
        {
            return await _workslotEmployeeRepository.GetWorkSlotEmployeeByEmployeeId(employeeId);
        }

        public async Task<object> CheckInWorkslotEmployee(Guid employeeId, DateTime? currentTime) => await _workslotEmployeeRepository.CheckInWorkslotEmployee(employeeId, currentTime);

        public async Task<string> ExportWorkSlotEmployeeReport(Guid departmentId) => await _workslotEmployeeRepository.ExportWorkSlotEmployeeReport(departmentId);
    }
}