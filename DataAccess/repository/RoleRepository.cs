using DataAccess.InterfaceRepository;

namespace DataAccess.Repository
{
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        private readonly MyDbContext _dbContext;

        public RoleRepository(MyDbContext context) : base(context)
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

        public async Task<object> GetAllRole()
        {
            return _dbContext.Roles.Where(r => r.IsDeleted == false).Select(r => new
            {
                r.ID,
                r.Name,
                r.IsDeleted
            });
        }
    }
}
