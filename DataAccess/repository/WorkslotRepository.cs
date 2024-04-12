using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace DataAccess.Repository
{
    public class WorkslotRepository : Repository<Workslot>, IWorkslotRepository
    {
        private readonly MyDbContext _dbContext;

        public WorkslotRepository(MyDbContext context) : base(context)
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

        public async Task<List<Workslot>> GenerateWorkSlotsForMonth(CreateWorkSlotRequest request)
        {
            List<Workslot> workSlots = new List<Workslot>();

            var dateStart = DateTime.ParseExact(request.month, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            var department = _dbContext.Departments.Include(d => d.WorkTrackSetting).ThenInclude(wts => wts.WorkDateSetting).Include(d => d.WorkTrackSetting).ThenInclude(wts => wts.WorkTimeSetting).FirstOrDefault(d => d.Id == request.departmentId);
            if (department == null)
            {
                throw new Exception("Team not existing");
            }

            var workTrackSetting = department.WorkTrackSetting;

            // Deserialize the WorkDateSetting to know which days are workdays
            DateStatusDTO workDays = JsonSerializer.Deserialize<DateStatusDTO>(workTrackSetting.WorkDateSetting.DateStatus);

            // Retrieve work time settings
            string fromHourMorning = workTrackSetting.WorkTimeSetting.FromHourMorning;
            string toHourMorning = workTrackSetting.WorkTimeSetting.ToHourMorning;
            string fromHourAfternoon = workTrackSetting.WorkTimeSetting.FromHourAfternoon;
            string toHourAfternoon = workTrackSetting.WorkTimeSetting.ToHourAfternoon;

            // Generate WorkSlots for the given month
            DateTime startDate = new DateTime(dateStart.Year, dateStart.Month, 1);
            DateTime endDate = startDate.AddMonths(1);

            for (DateTime date = startDate; date < endDate; date = date.AddDays(1))
            {
                string dayOfWeek = date.DayOfWeek.ToString();
                bool isWorkDay = (bool)typeof(DateStatusDTO).GetProperty(dayOfWeek).GetValue(workDays);
                bool isHoliday = await IsHoliday(_dbContext, date.ToString("yyyy/MM/dd"));

                if (isWorkDay && !isHoliday)
                {
                    // Create morning slot
                    Workslot morningSlot = new Workslot
                    {
                        Name = $"Morning Slot - {date.ToShortDateString()}",
                        IsMorning = true,
                        DateOfSlot = date,
                        FromHour = fromHourMorning,
                        ToHour = toHourMorning,
                        IsDeleted = false,
                        DepartmentId = request.departmentId,
                        Department = department
                    };

                    // Create afternoon slot
                    Workslot afternoonSlot = new Workslot
                    {
                        Name = $"Afternoon Slot - {date.ToShortDateString()}",
                        IsMorning = false,
                        DateOfSlot = date,
                        FromHour = fromHourAfternoon,
                        ToHour = toHourAfternoon,
                        IsDeleted = false,
                        DepartmentId = request.departmentId,
                        Department = department
                    };

                    workSlots.Add(morningSlot);
                    workSlots.Add(afternoonSlot);
                }
            }
            _dbContext.Workslots.AddRange(workSlots);
            await _dbContext.SaveChangesAsync();
            await RemoveDuplicateWorkSlots();
            return workSlots;
        }

        public async Task<int> RemoveDuplicateWorkSlots()
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    // Fetch all work slots
                    var workSlots = await _dbContext.Workslots.ToListAsync();

                    // Identify duplicates based on full criteria
                    var duplicatesFullCriteria = workSlots.GroupBy(ws => new { ws.Name, ws.DateOfSlot, ws.FromHour, ws.ToHour, ws.DepartmentId, ws.IsMorning })
                                                          .Where(g => g.Count() > 1)
                                                          .SelectMany(g => g.OrderBy(ws => ws.Id).Skip(1))  // Skip the first item as it's the original
                                                          .ToList();

                    // Identify duplicates based on Name and DepartmentId only
                    var duplicatesNameDepartment = workSlots.GroupBy(ws => new { ws.Name, ws.DepartmentId })
                                                             .Where(g => g.Count() > 1)
                                                             .SelectMany(g => g.OrderBy(ws => ws.Id).Skip(1))  // Skip the first item as it's the original
                                                             .ToList();

                    // Combine both sets of duplicates
                    var allDuplicates = duplicatesFullCriteria.Concat(duplicatesNameDepartment)
                                                              .Distinct()
                                                              .ToList();

                    // Find all WorkslotEmployee entries that are associated with the duplicate work slots
                    var workslotEmployeeIds = _dbContext.WorkslotEmployees
                                                        .Where(we => allDuplicates.Select(d => d.Id).Contains(we.WorkslotId))
                                                        .ToList();

                    // Remove WorkslotEmployee entries
                    _dbContext.WorkslotEmployees.RemoveRange(workslotEmployeeIds);

                    // Remove the duplicate work slots
                    _dbContext.Workslots.RemoveRange(allDuplicates);

                    // Save changes to the database
                    int changes = await _dbContext.SaveChangesAsync();

                    // Commit transaction if all commands succeed
                    transaction.Commit();

                    return changes;  // Return the number of changes made to the database
                }
                catch (Exception)
                {
                    // Rollback the transaction if an exception occurs
                    transaction.Rollback();
                    throw;  // Rethrow the exception to handle it further up the call stack if necessary
                }
            }
        }


        public async Task<List<object>> GetWorkSlotsForDepartment(CreateWorkSlotRequest request)
        {
            List<object> response = new List<object>();

            // Parse the month from the request
            var dateStart = DateTime.ParseExact(request.month, "yyyy/MM/dd", CultureInfo.InvariantCulture);

            // Retrieve the department and its associated WorkTrackSetting from the database
            var department = _dbContext.Departments
                .Include(d => d.WorkTrackSetting)
                .ThenInclude(wts => wts.WorkDateSetting)
                .FirstOrDefault(d => d.Id == request.departmentId);

            if (department == null)
            {
                throw new Exception("Team not existing");
            }

            var workTrackSetting = department.WorkTrackSetting;

            // Deserialize the WorkDateSetting to know which days are workdays
            DateStatusDTO workDays = JsonSerializer.Deserialize<DateStatusDTO>(workTrackSetting.WorkDateSetting.DateStatus);

            // Calculate the start and end date for the given month
            DateTime startDate = new DateTime(dateStart.Year, dateStart.Month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            // Retrieve all work slots for the department within the date range
            var workSlots = _dbContext.Workslots
                .Where(ws => ws.DepartmentId == request.departmentId && ws.DateOfSlot >= startDate && ws.DateOfSlot <= endDate)
                .ToList();

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                string dayOfWeek = date.DayOfWeek.ToString();
                bool isWorkDay = (bool)typeof(DateStatusDTO).GetProperty(dayOfWeek).GetValue(workDays);
                bool isHoliday = await IsHoliday(_dbContext, date.ToString("yyyy/MM/dd"));

                var slotsForDate = workSlots.Where(ws => ws.DateOfSlot.Date == date.Date).ToList();

                if (isWorkDay && slotsForDate.Count >= 2 && !isHoliday)
                {
                    // If it's a working day and has both morning and afternoon slots, combine them
                    var morningSlot = slotsForDate.First(ws => ws.IsMorning);
                    var afternoonSlot = slotsForDate.First(ws => !ws.IsMorning);
                    response.Add(new
                    {
                        title = "Working",
                        date = date.ToString("yyyy-MM-dd"),
                        startTime = morningSlot.FromHour,
                        endTime = afternoonSlot.ToHour
                    });
                }
                else if (isWorkDay && !isHoliday)
                {
                    // If it's a working day but has only one slot, add it
                    foreach (var slot in slotsForDate)
                    {
                        response.Add(new
                        {
                            title = "Working",
                            date = date.ToString("yyyy-MM-dd"),
                            startTime = slot.FromHour,
                            endTime = slot.ToHour
                        });
                    }
                }
                else if (isHoliday)
                {
                    // If it's not a working day, add a "not working" entry
                    response.Add(new
                    {
                        title = "Public Holiday",
                        date = date.ToString("yyyy-MM-dd"),
                        startTime = "00:00",
                        endTime = "00:00"
                    });
                } else
                {
                    // If it's not a working day, add a "not working" entry
                    response.Add(new
                    {
                        title = "Non working",
                        date = date.ToString("yyyy-MM-dd"),
                        startTime = "00:00",
                        endTime = "00:00"
                    });
                }
            }

            return response;
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
    }
}
