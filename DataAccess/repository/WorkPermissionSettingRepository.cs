using DataAccess.InterfaceRepository;

namespace DataAccess.Repository
{
    public class WorkPermissionSettingRepository : Repository<WorkPermissionSetting>, IWorkPermissionSettingRepository
    {
        private readonly MyDbContext _dbContext;

        public WorkPermissionSettingRepository(MyDbContext context) : base(context)
        {
            // You can add more specific methods here if needed
            _dbContext = context;
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
