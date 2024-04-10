using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

        public async Task<object> AddAsync(PostHolidayListDTO acc)
        {
            try
            {
                var newHolidayId = Guid.NewGuid();
                await _dbContext.DepartmentHolidays.AddAsync(new DepartmentHoliday() // have dbSaveChange inside method
                {
                    HolidayId = newHolidayId,
                    HolidayName = acc.HolidayName,
                    Description = acc.Description,
                    IsDeleted = false,
                    IsRecurring = true,
                    StartDate = DateTime.ParseExact(acc.StartDate, "yyyy/MM/dd", CultureInfo.InvariantCulture),
                    EndDate = DateTime.ParseExact(acc.EndDate, "yyyy/MM/dd", CultureInfo.InvariantCulture),
                });

                return new { message = "Add Holiday Sucessfully", newHolidayId };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
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
