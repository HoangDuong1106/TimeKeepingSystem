using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Transactions;

namespace DataAccess.Repository
{
    public class HolidayRepository : Repository<Holiday>, IHolidayRepository
    {
        private readonly MyDbContext _dbContext;
        private readonly IWorkslotRepository _workslotRepository;

        public HolidayRepository(MyDbContext context, IWorkslotRepository workslotRepository) : base(context)
        {
            // You can add more specific methods here if needed
            _dbContext = context;
            _workslotRepository = workslotRepository;
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
                    var newHolidayId = acc.HolidayId != Guid.Empty ? acc.HolidayId : Guid.NewGuid();
                    var newHoliday = new Holiday()
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

        public async Task<bool> SoftDelete(Guid[] holidayIds)
        {
            try
            {
                foreach (Guid holidayId in holidayIds)
                {
                    var holiday = await _dbContext.DepartmentHolidays.FindAsync(holidayId);
                    if (holiday == null) throw new Exception("Holiday not found.");

                    // Soft delete the holiday
                    holiday.IsDeleted = true;
                    _dbContext.DepartmentHolidays.Remove(holiday);
                    await _dbContext.SaveChangesAsync();

                    // Get all unique department IDs
                    var departmentIds = await _dbContext.Departments.Select(d => d.Id).ToListAsync();

                    // Extract start and end months
                    var startMonth = new DateTime(holiday.StartDate.Year, holiday.StartDate.Month, 1);
                    var endMonth = new DateTime(holiday.EndDate.Year, holiday.EndDate.Month, 1);

                    // Process each department and each month sequentially
                    var monthsToRegenerate = Enumerable.Range(0, (endMonth.Year - startMonth.Year) * 12 + endMonth.Month - startMonth.Month + 1)
                                                       .Select(offset => startMonth.AddMonths(offset)).ToList();

                    foreach (var deptId in departmentIds)
                    {
                        foreach (var month in monthsToRegenerate)
                        {
                            await RegenerateWorkSlotsForDepartment(deptId, month);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // Log and handle exceptions
                throw new Exception("Error during holiday deletion and slot regeneration: " + ex.Message);
            }
        }

        private async Task RegenerateWorkSlotsForDepartment(Guid departmentId, DateTime month)
        {
            var request = new CreateWorkSlotRequest
            {
                departmentId = departmentId,
                month = month.ToString("yyyy/MM/dd")
            };
            await _workslotRepository.GenerateWorkSlotsForMonth(request);
        }

    }
}
