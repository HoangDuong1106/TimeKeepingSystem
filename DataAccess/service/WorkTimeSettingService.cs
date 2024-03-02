using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using BusinessObject.Migrations;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class WorkTimeSettingService : IWorkTimeSettingService
    {
        private readonly IWorkTimeSettingRepository _workTimeSettingRepository;

        public WorkTimeSettingService(IWorkTimeSettingRepository workTimeSettingRepository)
        {
            _workTimeSettingRepository = workTimeSettingRepository;
        }

        // Implement the GetAllAsync method from IWorkTimeSettingService by calling the corresponding repository method
        public async Task<WorkTimeSetting> CreateAsync(WorkTimeSetting workTimeSetting) => await _workTimeSettingRepository.CreateAsync(workTimeSetting);
        public async Task<WorkTimeSetting> GetByIdAsync(Guid id) => await _workTimeSettingRepository.GetByIdAsync(id);
        public async Task<WorkTimeSetting> UpdateAsync(WorkTimeSetting workTimeSetting) => await _workTimeSettingRepository.UpdateAsync(workTimeSetting);
        public async Task<bool> SoftDeleteAsync(Guid id) => await _workTimeSettingRepository.SoftDeleteAsync(id);
        public async Task<List<WorkTimeSetting>> GetAllAsync() => await _workTimeSettingRepository.GetAllAsync();
    }
    
}