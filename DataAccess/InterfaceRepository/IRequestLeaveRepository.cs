using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { public interface IRequestLeaveRepository { 
        Task<bool> AddAsync(RequestLeaveDTO a);
        Task<object> ApproveRequestAndChangeWorkslotEmployee(Guid requestId);
        Task<object> CancelApprovedLeaveRequest(Guid requestId, string reason);
        Task<object> CreateRequestLeave(LeaveRequestDTO dto, Guid employeeId);
        Task<object> DeleteLeaveRequestIfNotApproved(Guid requestId);
        Task<bool> EditRequestLeave(LeaveRequestDTO dto, Guid employeeId);
        Task<List<RequestLeaveDTO>> GetAllAsync();
        Task<List<object>> GetApprovedLeaveDaysByTypeAsync(Guid employeeId);
        object GetRequestLeaveAllEmployee(string? nameSearch, int status);
        object GetRequestLeaveByRequestId(Guid requestId);
        object GetRequestLeaveOfEmployeeById(Guid employeeId);
        Task<WorkDateSettingDTO> GetWorkDateSettingFromEmployeeId(Guid employeeId);
        Task<bool> SoftDeleteAsync(Guid id); } }