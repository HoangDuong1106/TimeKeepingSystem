using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { public interface IRequestLeaveRepository { 
        Task<bool> AddAsync(RequestLeaveDTO a);
        Task<object> ApproveRequestAndChangeWorkslotEmployee(Guid requestId);
        Task<object> CreateRequestLeave(LeaveRequestDTO dto, Guid employeeId);
        Task<bool> EditRequestLeave(LeaveRequestDTO dto, Guid employeeId);
        Task<List<RequestLeaveDTO>> GetAllAsync();
        object GetRequestLeaveAllEmployee(string? nameSearch, int status);
        object GetRequestLeaveByRequestId(Guid requestId);
        object GetRequestLeaveOfEmployeeById(Guid employeeId);
        Task<WorkDateSettingDTO> GetWorkDateSettingFromEmployeeId(Guid employeeId);
        Task<bool> SoftDeleteAsync(Guid id); } }