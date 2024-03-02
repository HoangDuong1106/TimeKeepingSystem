using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class WorkTimeSettingRepository : Repository<WorkTimeSetting>, IWorkTimeSettingRepository
    {
        private readonly MyDbContext _dbContext;

        public WorkTimeSettingRepository(MyDbContext context) : base(context)
        {
            // You can add more specific methods here if needed
            _dbContext = context;
        }

        // Create
        public async Task<WorkTimeSetting> CreateAsync(WorkTimeSetting workTimeSetting)
        {
            await _dbContext.WorkTimeSettings.AddAsync(workTimeSetting);
            await _dbContext.SaveChangesAsync();
            return workTimeSetting;
        }

        // Read
        public async Task<WorkTimeSetting> GetByIdAsync(Guid id)
        {
            return await _dbContext.WorkTimeSettings.FindAsync(id);
        }

        // Update
        public async Task<WorkTimeSetting> UpdateAsync(WorkTimeSetting workTimeSetting)
        {
            _dbContext.WorkTimeSettings.Update(workTimeSetting);
            await _dbContext.SaveChangesAsync();
            return workTimeSetting;
        }

        // Delete
        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var workTimeSetting = await _dbContext.WorkTimeSettings.FindAsync(id);
            if (workTimeSetting == null)
            {
                return false;
            }

            workTimeSetting.IsDeleted = true;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Get All
        public async Task<List<WorkTimeSetting>> GetAllAsync()
        {
            return await _dbContext.WorkTimeSettings.Where(w => !w.IsDeleted).ToListAsync();
        }

    }
}
