using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { public interface ILeaveTypeRepository { Task<bool> AddAsync(LeaveTypeDTO a); Task<List<LeaveTypeDTO>> GetAllAsync(); Task<bool> SoftDeleteAsync(Guid id); } }