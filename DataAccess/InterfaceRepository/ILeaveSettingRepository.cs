using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { public interface ILeaveSettingRepository { Task<bool> AddAsync(LeaveSettingDTO a); Task<List<LeaveSettingDTO>> GetAllAsync(); Task<bool> SoftDeleteAsync(Guid id); Task<object> UpdateLeaveSettingAsync(LeaveSettingDTO updatedLeaveSetting); } }