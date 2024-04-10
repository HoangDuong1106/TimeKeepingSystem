using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { public interface IRequestLeaveRepository { 
        Task<bool> AddAsync(RequestLeaveDTO a);
        Task<object> ApproveRequestAndChangeWorkslotEmployee(Guid requestId, Guid? employeeIdDecider);
        Task<object> CancelApprovedLeaveRequest(RequestReasonDTO request);
        Task<object> CreateRequestLeave(LeaveRequestDTO dto, Guid employeeId);
        Task<object> DeleteLeaveRequestIfNotApproved(Guid requestId, Guid? employeeIdDecider);
        Task<bool> EditRequestLeave(LeaveRequestDTO dto, Guid employeeId);
        Task<List<RequestLeaveDTO>> GetAllAsync();
        Task<List<object>> GetApprovedLeaveDaysByTypeAsync(Guid employeeId);
        object GetRequestLeaveAllEmployee(string? nameSearch, int status, Guid? employeeId);
        object GetRequestLeaveByRequestId(Guid requestId);
        object GetRequestLeaveOfEmployeeById(Guid employeeId);
        Task<WorkDateSettingDTO> GetWorkDateSettingFromEmployeeId(Guid employeeId);
        Task<object> RejectLeaveRequest(RequestReasonDTO requestObj);
        Task<bool> SendLeaveRequestStatusToFirebase(Guid requestId, string path);
        Task<bool> SoftDeleteAsync(Guid id); } }