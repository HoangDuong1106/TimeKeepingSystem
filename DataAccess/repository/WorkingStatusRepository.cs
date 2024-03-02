using BusinessObject.DTO;
using DataAccess.InterfaceRepository;

namespace DataAccess.Repository
{
    public class WorkingStatusRepository : Repository<WorkingStatus>, IWorkingStatusRepository
    {
        private readonly MyDbContext _dbContext;

        public WorkingStatusRepository(MyDbContext context) : base(context)
        {
            // You can add more specific methods here if needed
            _dbContext = context;
        }

        public async Task<object> Create(WorkingStatusDTO newWorkingStatus)
        {
            WorkingStatus workingStatus = new WorkingStatus()
            {
                Id = Guid.NewGuid(),
                Name = newWorkingStatus.Name,
                IsDeleted = false
            };
            _dbContext.WorkingStatuses.Add(workingStatus);
            await _dbContext.SaveChangesAsync();
            return workingStatus;
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            try
            {
                await base.SoftDeleteAsync(id);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
