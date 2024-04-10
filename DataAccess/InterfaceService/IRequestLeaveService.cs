using BusinessObject.DTO;
using System;

namespace DataAccess.InterfaceService
{
    public interface IRequestLeaveService
    {
        Task<object> ApproveRequestAndChangeWorkslotEmployee(Guid requestId, Guid? employeeIdDecider);
        Task<object> CreateRequestLeave(LeaveRequestDTO dto, Guid employeeId);
        Task<bool> EditRequestLeave(LeaveRequestDTO dto, Guid employeeId);
        object GetRequestLeaveAllEmployee(string? nameSearch, int status, Guid? employeeId);
        object GetRequestLeaveByRequestId(Guid requestId);

        // Empty interface
        object GetRequestLeaveOfEmployeeById(Guid employeeId);
        Task<WorkDateSettingDTO> GetWorkDateSettingFromEmployeeId(Guid employeeId);
    }
}
