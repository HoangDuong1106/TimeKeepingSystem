using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class DepartmentHolidayExceptionRepository : Repository<DepartmentHolidayException>, IDepartmentHolidayExceptionRepository
    {
        private readonly MyDbContext _dbContext;

        public DepartmentHolidayExceptionRepository(MyDbContext context) : base(context)
        {
            // You can add more specific methods here if needed
            _dbContext = context;
        }

        public async Task<List<DepartmentHolidayExceptionDTO>> GetAllAsync()
        {
            var ass = await base.GetAllAsync();
            return await ass.Select(a => new DepartmentHolidayExceptionDTO
            {
                ExceptionId = a.ExceptionId,
                HolidayId = a.HolidayId,
                ExceptionDate = a.ExceptionDate,
                Reason = a.Reason,
                IsDeleted = a.IsDeleted
            }).ToListAsync();
        }

        public async Task<bool> AddAsync(DepartmentHolidayExceptionDTO a)
        {
            try
            {
                await base.AddAsync(new DepartmentHolidayException() // have dbSaveChange inside method
                {
                    ExceptionId = a.ExceptionId,
                    HolidayId = a.HolidayId,
                    ExceptionDate = a.ExceptionDate,
                    Reason = a.Reason,
                    IsDeleted = a.IsDeleted,
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
