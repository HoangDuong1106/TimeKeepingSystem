using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class RequestLeaveService : IRequestLeaveService
    {
        private readonly IRequestLeaveRepository _requestLeaveRepository;

        public RequestLeaveService(IRequestLeaveRepository requestLeaveRepository)
        {
            _requestLeaveRepository = requestLeaveRepository;
        }

        // Implement the GetAllAsync method from IRequestLeaveService by calling the corresponding repository method
        public async Task<List<RequestLeaveDTO>> GetAllAsync()
        {
            return await _requestLeaveRepository.GetAllAsync();
        }

        public object GetRequestLeaveOfEmployeeById(Guid employeeId)
        {
            return _requestLeaveRepository.GetRequestLeaveOfEmployeeById(employeeId);
        }

        public async Task<WorkDateSettingDTO> GetWorkDateSettingFromEmployeeId(Guid employeeId)
        {
            return await _requestLeaveRepository.GetWorkDateSettingFromEmployeeId(employeeId);
        }

        public async Task<bool> EditRequestLeave(LeaveRequestDTO dto, Guid employeeId)
        {
            return await _requestLeaveRepository.EditRequestLeave(dto, employeeId);
        }

        public async Task<object> CreateRequestLeave(LeaveRequestDTO dto, Guid employeeId)
        {
            return await _requestLeaveRepository.CreateRequestLeave(dto, employeeId);
        }

        public object GetRequestLeaveAllEmployee(string? nameSearch, int status)
        {
            return _requestLeaveRepository.GetRequestLeaveAllEmployee(nameSearch, status);
        }

        public async Task<object> ApproveRequestAndChangeWorkslotEmployee(Guid requestId, Guid? employeeIdDecider)
        {
            return await _requestLeaveRepository.ApproveRequestAndChangeWorkslotEmployee(requestId, employeeIdDecider);
        }

        public object GetRequestLeaveByRequestId(Guid requestId)
        {
            return _requestLeaveRepository.GetRequestLeaveByRequestId(requestId);
        }
    }
}