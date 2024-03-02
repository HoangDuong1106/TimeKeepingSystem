using DataAccess.InterfaceRepository;

namespace DataAccess.Repository
{
    public class RequestRepository : Repository<Request>, IRequestRepository
    {
        private readonly MyDbContext _dbContext;

        public RequestRepository(MyDbContext context) : base(context)
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
