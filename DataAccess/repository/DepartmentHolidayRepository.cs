using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class DepartmentHolidayRepository : Repository<DepartmentHoliday>, IDepartmentHolidayRepository
    {
        private readonly MyDbContext _dbContext;

        public DepartmentHolidayRepository(MyDbContext context) : base(context)
        {
            // You can add more specific methods here if needed
            _dbContext = context;
        }

        public async Task<List<DepartmentHolidayDTO>> GetAllAsync()
        {
            var ass = await base.GetAllAsync();
            return await ass.Select(a => new DepartmentHolidayDTO
            {
                HolidayId = a.HolidayId,
                HolidayName = a.HolidayName,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Description = a.Description,
                IsRecurring = a.IsRecurring,
                IsDeleted = a.IsDeleted
            }).ToListAsync();
        }

        public async Task<bool> AddAsync(DepartmentHolidayDTO a)
        {
            try
            {
                await base.AddAsync(new DepartmentHoliday() // have dbSaveChange inside method
                {
                    HolidayId = (Guid)a.HolidayId,
                    HolidayName = a.HolidayName,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Description = a.Description,
                    IsRecurring = (bool)a.IsRecurring,
                    IsDeleted = (bool)a.IsDeleted
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
