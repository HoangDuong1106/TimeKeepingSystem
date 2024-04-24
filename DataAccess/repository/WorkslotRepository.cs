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

                    // Mark duplicates as deleted
                    foreach (var dup in allDuplicates)
                    {
                        dup.IsDeleted = true;
                    }

                    // Find all WorkslotEmployee entries that are associated with the duplicate work slots, have null Workslot, or have null Employee, and mark them as deleted
                    var workslotEmployees = _dbContext.WorkslotEmployees.Include(we => we.Workslot).Include(we => we.Employee)
                                                      .Where(we => allDuplicates.Select(d => d.Id).Contains(we.WorkslotId) || we.Workslot == null || we.Employee == null)
                                                      .ToList();

                    foreach (var we in workslotEmployees)
                    {
                        we.IsDeleted = true;
                    }

                    // Save changes to the database
                    int changes = await _dbContext.SaveChangesAsync();

                    // Commit transaction if all commands succeed
                    transaction.Commit();

                    return changes;  // Return the number of changes made to the database
                }
                catch (Exception ex)
                {
                    // Rollback the transaction if an exception occurs
                    transaction.Rollback();
                    throw new Exception("Failed to remove duplicate work slots: " + ex.Message);
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

        public async Task<List<object>> GetWorkSlotsForDepartmentOrEmployee(CreateWorkSlotRequest request)
        {
            DateTime startDate = DateTime.ParseExact(request.month, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            if (request.employeeId.HasValue)
            {
                var employee = await _dbContext.Employees
                    .Include(e => e.UserAccount).ThenInclude(ua => ua.Role)
                    .FirstOrDefaultAsync(e => e.Id == request.employeeId.Value);

                if (employee == null)
                    throw new Exception("Employee not found.");

                var role = employee.UserAccount.Role.Name;

                // Determine the response based on the role
                switch (role)
                {
                    case "HR":
                        return await GenerateHRView(request.departmentId.GetValueOrDefault(), startDate, endDate);

                    case "Manager":
                        if (employee.DepartmentId.HasValue)
                        {
                            return await GenerateManagerView(employee.DepartmentId.Value, startDate, endDate);
                        }
                        else
                        {
                            throw new Exception("Manager must be assigned to a department.");
                        }

                    default:
                        return await GenerateEmployeeView(employee.Id, startDate, endDate);
                }
            }
            else if (request.departmentId.HasValue)
            {
                return await GenerateHRView(request.departmentId.Value, startDate, endDate);
            }
            else
            {
                throw new Exception("Either departmentId or employeeId must be provided.");
            }
        }

        public async Task<List<object>> GetWorkSlotsForPersonal(CreateWorkSlotRequest request)
        {
            DateTime startDate = DateTime.ParseExact(request.month, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            DateTime endDate = new DateTime(startDate.Year, startDate.Month, DateTime.DaysInMonth(startDate.Year, startDate.Month));

            if (request.employeeId.HasValue)
            {
                var employee = await _dbContext.Employees
                    .Include(e => e.UserAccount).ThenInclude(ua => ua.Role)
                    .FirstOrDefaultAsync(e => e.Id == request.employeeId.Value);

                if (employee == null)
                    throw new Exception("Employee not found.");

                // Determine the response based on the role
                return await GenerateEmployeeView(employee.Id, startDate, endDate);
            }
            throw new Exception("EmployeeId cannot be null.");
        }


        private async Task<List<object>> GenerateHRView(Guid departmentId, DateTime startDate, DateTime endDate)
        {
            var workDays = await GetWorkDays(departmentId);
            var response = new List<object>();

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var slots = await GenerateTeamSlots(departmentId, date, workDays);
                response.AddRange(slots);
            }

            return response; // Directly reuse existing functionality for HR, which already handles department-wide slots.
        }

        private async Task<List<object>> GenerateManagerView(Guid departmentId, DateTime startDate, DateTime endDate)
        {
            var workDays = await GetWorkDays(departmentId);
            var response = new List<object>();

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var slots = await GenerateTeamSlots(departmentId, date, workDays);
                response.AddRange(slots);
            }

            return response;
        }

        private async Task<List<object>> GenerateEmployeeView(Guid employeeId, DateTime startDate, DateTime endDate)
        {
            var employee = await _dbContext.Employees.Include(e => e.Department).FirstOrDefaultAsync(e => e.Id == employeeId);
            if (employee == null || employee.Department == null)
                throw new Exception("Employee or their department not found.");

            var workDays = await GetWorkDays(employee.Department.Id);
            var response = new List<object>();

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var slots = await GeneratePersonalSlots(employeeId, date, workDays);
                var filteredSlots = await FilterSlotsBeforeAggregation(slots);
                response.AddRange(filteredSlots);
            }

            return AggregateWorkSlots(response);
        }

        private async Task<DateStatusDTO> GetWorkDays(Guid departmentId)
        {
            var workTrackSetting = await _dbContext.Departments
                .Where(d => d.Id == departmentId)
                .Select(d => d.WorkTrackSetting.WorkDateSetting.DateStatus)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(workTrackSetting))
                throw new Exception("Work day settings not found for the department.");

            return JsonSerializer.Deserialize<DateStatusDTO>(workTrackSetting);
        }

        private async Task<List<object>> GenerateTeamSlots(Guid departmentId, DateTime date, DateStatusDTO workDays)
        {
            List<object> slots = new List<object>();
            bool isWorkDay = (bool)typeof(DateStatusDTO).GetProperty(date.DayOfWeek.ToString())?.GetValue(workDays);
            bool isHoliday = await IsHoliday(_dbContext, date.ToString("yyyy/MM/dd"));

            var workSlots = _dbContext.Workslots
                .Where(ws => ws.DateOfSlot.Date == date && ws.DepartmentId == departmentId)
                .ToList();

            var leaveRequests = _dbContext.Requests
                .Include(r => r.RequestLeave).ThenInclude(rl => rl.WorkslotEmployees).ThenInclude(we => we.Workslot).Include(r => r.EmployeeSendRequest)
                .Where(r => r.EmployeeSendRequest.DepartmentId == departmentId && r.Status == RequestStatus.Approved && r.RequestLeave.FromDate <= date && r.RequestLeave.ToDate >= date)
                .ToList();

            var overtimeRequests = _dbContext.Requests
                .Include(r => r.RequestOverTime).Include(r => r.EmployeeSendRequest)
                .Where(r => r.EmployeeSendRequest.DepartmentId == departmentId && r.Status == RequestStatus.Approved && r.RequestOverTime.DateOfOverTime == date)
                .ToList();

            // Compile information for the department for each day
            if (!isHoliday)
            {
                foreach (var slot in workSlots)
                {
                    slots.Add(new
                    {
                        title = "Working",
                        date = date.ToString("yyyy-MM-dd"),
                        startTime = slot.FromHour,
                        endTime = slot.ToHour
                    });
                }

                foreach (var leave in leaveRequests)
                {
                    slots.Add(new
                    {
                        title = "Leave",
                        date = date.ToString("yyyy-MM-dd"),
                        startTime = leave.RequestLeave.WorkslotEmployees.FirstOrDefault(we => we.Workslot.DateOfSlot.Date == date)?.Workslot?.FromHour,
                        endTime = leave.RequestLeave.WorkslotEmployees.FirstOrDefault(we => we.Workslot.DateOfSlot.Date == date)?.Workslot?.ToHour,
                        employeeName = $"{leave.EmployeeSendRequest.FirstName} {leave.EmployeeSendRequest.LastName}"
                    });
                }

                foreach (var ot in overtimeRequests)
                {
                    slots.Add(new
                    {
                        title = "Overtime",
                        date = date.ToString("yyyy-MM-dd"),
                        startTime = ot.RequestOverTime.FromHour.ToString("HH:mm"),
                        endTime = ot.RequestOverTime.ToHour.ToString("HH:mm"),
                        employeeName = $"{ot.EmployeeSendRequest.FirstName} {ot.EmployeeSendRequest.LastName}"
                    });
                }
            }
            else
            {
                slots.Add(new
                {
                    title = "Public Holiday",
                    date = date.ToString("yyyy-MM-dd"),
                    startTime = "00:00",
                    endTime = "00:00"
                });
            }

            return slots;
        }

        private async Task<List<object>> GeneratePersonalSlots(Guid employeeId, DateTime date, DateStatusDTO workDays)
        {
            List<object> slots = new List<object>();
            bool isWorkDay = (bool)typeof(DateStatusDTO).GetProperty(date.DayOfWeek.ToString())?.GetValue(workDays);
            bool isHoliday = await IsHoliday(_dbContext, date.ToString("yyyy/MM/dd"));

            var departmentId = _dbContext.Employees.FirstOrDefault(e => e.Id == employeeId)?.DepartmentId;

            var workSlots = _dbContext.Workslots
                .Where(ws => ws.DateOfSlot.Date == date && ws.DepartmentId == departmentId)
                .ToList();

            var leaveRequests = _dbContext.Requests
                .Include(r => r.RequestLeave).ThenInclude(rl => rl.WorkslotEmployees).ThenInclude(we => we.Workslot).Include(r => r.EmployeeSendRequest)
                .Where(r => r.EmployeeSendRequestId == employeeId && r.Status == RequestStatus.Approved && r.RequestLeave.FromDate <= date && r.RequestLeave.ToDate >= date)
                .ToList();

            var overtimeRequests = _dbContext.Requests
                .Include(r => r.RequestOverTime).Include(r => r.EmployeeSendRequest)
                .Where(r => r.EmployeeSendRequestId == employeeId && r.Status == RequestStatus.Approved && r.RequestOverTime.DateOfOverTime == date)
                .ToList();

            if (!isHoliday)
            {
                if (workSlots.Any())
                {
                    foreach (var slot in workSlots)
                    {
                        slots.Add(new
                        {
                            title = "Working",
                            date = date.ToString("yyyy-MM-dd"),
                            startTime = slot.FromHour,
                            endTime = slot.ToHour,
                            period = slot.IsMorning ? "Morning" : "Afternoon"
                        });
                    }
                }
                else
                {
                    // Non-working day with possible overtime
                    slots.Add(new
                    {
                        title = "Non-working",
                        date = date.ToString("yyyy-MM-dd"),
                        startTime = "00:00",
                        endTime = "00:00",
                        period = ""
                    });
                }

                foreach (var leave in leaveRequests)
                {
                    slots.Add(new
                    {
                        title = "Leave",
                        date = date.ToString("yyyy-MM-dd"),
                        startTime = leave.RequestLeave.WorkslotEmployees.FirstOrDefault(we => we.Workslot.DateOfSlot.Date == date)?.Workslot?.FromHour,
                        endTime = leave.RequestLeave.WorkslotEmployees.FirstOrDefault(we => we.Workslot.DateOfSlot.Date == date)?.Workslot?.ToHour,
                        period = (bool)(leave.RequestLeave.WorkslotEmployees.FirstOrDefault(we => we.Workslot.DateOfSlot.Date == date)?.Workslot?.IsMorning) ? "Morning" : "Afternoon"
                    });
                }

                foreach (var ot in overtimeRequests)
                {
                    slots.Add(new
                    {
                        title = "Overtime",
                        date = date.ToString("yyyy-MM-dd"),
                        startTime = ot.RequestOverTime.FromHour.ToString("HH:mm"),
                        endTime = ot.RequestOverTime.ToHour.ToString("HH:mm"),
                        employeeName = $"{ot.EmployeeSendRequest.FirstName} {ot.EmployeeSendRequest.LastName}",
                        period = ""
                    });
                }
            }
            else
            {
                slots.Add(new
                {
                    title = "Public Holiday",
                    date = date.ToString("yyyy-MM-dd"),
                    startTime = "00:00",
                    endTime = "00:00",
                    period = ""
                });
            }

            return slots;
        }

        private List<object> AggregateWorkSlots(List<object> slots)
        {
            var aggregatedSlots = new List<object>();
            var groupedByDateAndTitle = slots.GroupBy(
                slot => new { Date = ((dynamic)slot).date, Title = ((dynamic)slot).title },
                (key, g) => new
                {
                    Date = key.Date,
                    Title = key.Title,
                    Slots = g.ToList()
                });

            foreach (var group in groupedByDateAndTitle)
            {
                DateTime minStartTime = DateTime.MaxValue;
                DateTime maxEndTime = DateTime.MinValue;
                HashSet<string> periods = new HashSet<string>();

                foreach (var slot in group.Slots)
                {
                    try
                    {
                        var startTimeString = ((dynamic)slot).startTime as string;
                        var endTimeString = ((dynamic)slot).endTime as string;
                        var period = ((dynamic)slot).period as string;

                        if (!string.IsNullOrWhiteSpace(startTimeString) && !string.IsNullOrWhiteSpace(endTimeString))
                        {
                            var startTime = DateTime.ParseExact(startTimeString, "HH:mm", CultureInfo.InvariantCulture);
                            var endTime = DateTime.ParseExact(endTimeString, "HH:mm", CultureInfo.InvariantCulture);

                            if (startTime < minStartTime)
                            {
                                minStartTime = startTime;
                            }
                            if (endTime > maxEndTime)
                            {
                                maxEndTime = endTime;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(period))
                        {
                            periods.Add(period);
                        }
                    }
                    catch (FormatException ex)
                    {
                        // Log the error or handle it as needed
                        continue;
                    }
                }

                string aggregatedPeriod = periods.Count > 1 ? "Fullday" : periods.FirstOrDefault() ?? "";

                if (minStartTime != DateTime.MaxValue && maxEndTime != DateTime.MinValue)
                {
                    aggregatedSlots.Add(new
                    {
                        title = group.Title,
                        date = group.Date,
                        startTime = minStartTime.ToString("HH:mm"),
                        endTime = maxEndTime.ToString("HH:mm"),
                        period = aggregatedPeriod
                    });
                }
            }

            return aggregatedSlots;
        }


        private async Task<bool> IsHoliday(string dateString)
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
            var isHoliday = await _dbContext.DepartmentHolidays
                                           .AnyAsync(h => h.StartDate <= date && h.EndDate >= date && !h.IsDeleted);

            return isHoliday;
        }

        private async Task<List<object>> FilterSlotsBeforeAggregation(List<object> slots)
        {
            var filteredSlots = new List<object>();
            var groupedByDate = slots.GroupBy(
                slot => ((dynamic)slot).date,
                (date, g) => new
                {
                    Date = date,
                    Slots = g.ToList()
                });

            foreach (var group in groupedByDate)
            {
                // Group slots by period, including those with empty period strings
                var slotsByPeriod = group.Slots.GroupBy(
                    slot => ((dynamic)slot).period,
                    (period, periodSlots) => new
                    {
                        Period = period,
                        Slots = periodSlots.ToList()
                    });

                foreach (var periodGroup in slotsByPeriod)
                {
                    if (!string.IsNullOrWhiteSpace(periodGroup.Period))
                    {
                        // Apply filtering only to slots with non-empty periods
                        if (periodGroup.Slots.Count(slot => ((dynamic)slot).title == "Leave") > 0)
                        {
                            // If there's a leave slot for this period, filter out any working slots.
                            filteredSlots.AddRange(periodGroup.Slots.Where(slot => ((dynamic)slot).title != "Working"));
                        }
                        else
                        {
                            // If there's no leave, add all slots as usual.
                            filteredSlots.AddRange(periodGroup.Slots);
                        }
                    }
                    else
                    {
                        // If the period string is empty, add all such slots without filtering.
                        filteredSlots.AddRange(periodGroup.Slots);
                    }
                }
            }

            return filteredSlots;
        }

    }
}
