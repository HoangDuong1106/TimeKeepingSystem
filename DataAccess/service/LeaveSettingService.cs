using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class LeaveSettingService : ILeaveSettingService
    {
        private readonly ILeaveSettingRepository _leaveSettingRepository;

        public LeaveSettingService(ILeaveSettingRepository leaveSettingRepository)
        {
            _leaveSettingRepository = leaveSettingRepository;
        }

        // Implement the GetAllAsync method from ILeaveSettingService by calling the corresponding repository method
        public async Task<List<LeaveSettingDTO>> GetAllAsync()
        {
            return await _leaveSettingRepository.GetAllAsync();
        }

        public async Task<object> UpdateLeaveSettingAsync(LeaveSettingDTO updatedLeaveSetting)
        {
            return await _leaveSettingRepository.UpdateLeaveSettingAsync(updatedLeaveSetting);
        }
    }
}