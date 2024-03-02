using System;

namespace DataAccess.InterfaceService
{
    public interface IWorkTimeSettingService
    {
        // Empty interface
        Task<WorkTimeSetting> CreateAsync(WorkTimeSetting workTimeSetting);
        Task<List<WorkTimeSetting>> GetAllAsync();
        Task<WorkTimeSetting> GetByIdAsync(Guid id);
        Task<bool> SoftDeleteAsync(Guid id);
        Task<WorkTimeSetting> UpdateAsync(WorkTimeSetting workTimeSetting);
    }
}
