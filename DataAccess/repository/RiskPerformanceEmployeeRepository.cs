using DataAccess.InterfaceRepository;

namespace DataAccess.Repository
{
    public class RiskPerformanceEmployeeRepository : Repository<RiskPerformanceEmployee>, IRiskPerformanceEmployeeRepository
    {
        private readonly MyDbContext _dbContext;

        public RiskPerformanceEmployeeRepository(MyDbContext context) : base(context)
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
