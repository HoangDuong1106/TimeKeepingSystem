using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class LeaveTypeRepository : Repository<LeaveType>, ILeaveTypeRepository
    {
        private readonly MyDbContext _dbContext;

        public LeaveTypeRepository(MyDbContext context) : base(context)
        {
            // You can add more specific methods here if needed
            _dbContext = context;
        }

        public async Task<List<LeaveTypeDTO>> GetAllAsync()
        {
            var ass = await base.GetAllAsync();
            return await ass.Select(a => new LeaveTypeDTO
            {
                Id = a.Id,
                Name = a.Name,
                AllowedDays = a.AllowedDays,
                LeaveCycle = a.LeaveCycle,
                CanCarryForward = a.CanCarryForward,
                TotalBalance = a.TotalBalance,
                IsDeleted = a.IsDeleted
            }).ToListAsync();
        }

        public async Task<bool> AddAsync(LeaveTypeDTO a)
        {
            try
            {
                await base.AddAsync(new LeaveType() // have dbSaveChange inside method
                {
                    Id = a.Id,
                    Name = a.Name,
                    AllowedDays = a.AllowedDays,
                    LeaveCycle = a.LeaveCycle,
                    CanCarryForward = a.CanCarryForward,
                    TotalBalance = a.TotalBalance,
                    IsDeleted = a.IsDeleted
                });
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
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
