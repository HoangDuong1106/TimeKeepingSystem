using BusinessObject.DTO;
using System;

namespace DataAccess.InterfaceService
{
    public interface ILeaveSettingService
    {
        Task<List<LeaveSettingDTO>> GetAllAsync();

        // Empty interface
        Task<object> UpdateLeaveSettingAsync(LeaveSettingDTO updatedLeaveSetting);
    }
}
