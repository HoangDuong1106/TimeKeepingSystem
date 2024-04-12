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
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    var newHolidayId = Guid.NewGuid();
                    var newHoliday = new DepartmentHoliday()
                    {
                        HolidayId = newHolidayId,
                        HolidayName = acc.HolidayName,
                        Description = acc.Description,
                        IsDeleted = false,
                        IsRecurring = true,
                        StartDate = DateTime.ParseExact(acc.StartDate, "yyyy/MM/dd", CultureInfo.InvariantCulture),
                        EndDate = DateTime.ParseExact(acc.EndDate, "yyyy/MM/dd", CultureInfo.InvariantCulture),
                    };

                    // Add the new holiday
                    await _dbContext.DepartmentHolidays.AddAsync(newHoliday);

                    // Fetch and delete work slots that fall within the holiday's date range for the relevant department(s)
                    var workSlotsToDelete = _dbContext.Workslots
                        .Where(ws => ws.DateOfSlot >= newHoliday.StartDate && ws.DateOfSlot <= newHoliday.EndDate)
                        .ToList();

                    // Find all associated WorkslotEmployee entries
                    var workslotEmployeeIds = _dbContext.WorkslotEmployees
                                                        .Where(we => workSlotsToDelete.Select(ws => ws.Id).Contains(we.WorkslotId))
                                                        .ToList();

                    // Remove WorkslotEmployee entries
                    _dbContext.WorkslotEmployees.RemoveRange(workslotEmployeeIds);

                    // Remove the duplicate work slots
                    _dbContext.Workslots.RemoveRange(workSlotsToDelete);

                    // Save changes to the database
                    await _dbContext.SaveChangesAsync();

                    // Commit the transaction
                    transaction.Commit();

                    return new { message = "Add Holiday Successfully", newHolidayId = newHolidayId };
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to add holiday: " + ex.Message, ex);
            }
        }

        public async Task<bool> IsHoliday(MyDbContext dbContext, string dateString)
        {
            // Parse the date from the string
            DateTime date;
            try
            {
                date = DateTime.ParseExact(dateString, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid date format. Please use 'yyyy/MM/dd'.");
            }

            // Check if the date is a holiday in any department
            var isHoliday = await dbContext.DepartmentHolidays
                                           .AnyAsync(h => h.StartDate <= date && h.EndDate >= date && !h.IsDeleted);

            return isHoliday;
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
