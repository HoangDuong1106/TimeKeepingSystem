using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace DataAccess.Repository
{
    public class WorkslotEmployeeRepository : Repository<WorkslotEmployee>, IWorkslotEmployeeRepository
    {
        private readonly MyDbContext _dbContext;
        private readonly IAttendanceStatusRepository _attendanceStatusRepository;
        private readonly ITeamRepository _departmentRepository;
        private Dictionary<string, string> coefficients;

        public WorkslotEmployeeRepository(MyDbContext context, IAttendanceStatusRepository attendanceStatusRepository, ITeamRepository departmentRepository) : base(context)
        {
            // You can add more specific methods here if needed
            _dbContext = context;
            _attendanceStatusRepository = attendanceStatusRepository;
            _departmentRepository = departmentRepository;
            coefficients = new Dictionary<string, string>
            {
                { "holiday", "x3" },
                { "nonWorkingDay", "x2" },
                { "normalDay", "x1.5" }
            };
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

        public class WorkSlotEmployeeId
        {
            public Guid WorkSlotId { get; set; }
            public Guid EmployeeId { get; set; }
        }

        public async Task<object> GenerateWorkSlotEmployee(CreateWorkSlotRequest request)
        {
            // Parse the month from the request
            var dateStart = DateTime.ParseExact(request.month, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            DateTime startDate = new DateTime(dateStart.Year, dateStart.Month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            // Fetch all employees of the department
            var employees = _dbContext.Employees
                .Where(e => e.DepartmentId == request.departmentId)
                .ToList();

            // Fetch all work slots for the department within the month's date range
            var workSlots = _dbContext.Workslots
                .Where(ws => ws.DepartmentId == request.departmentId && ws.DateOfSlot >= startDate && ws.DateOfSlot <= endDate)
                //.Where(ws => ws.)
                .ToList();

            List<WorkslotEmployee> newWorkSlotEmployees = new List<WorkslotEmployee>();
            var existingWorkslotEmployee = _dbContext.WorkslotEmployees.Where(w => w.IsDeleted == false).Select(w => new WorkSlotEmployeeId()
            {
                WorkSlotId = w.WorkslotId,
                EmployeeId = w.EmployeeId
            });

            // Combine each work slot with each employee to generate WorkSlotEmployee
            var attandance = _attendanceStatusRepository.GetAllAsync().Result.Find(a => a.Name == "Not Work Yet");
            var att = _dbContext.AttendanceStatuses.FirstOrDefault(a => a.Id == attandance.Id);
            foreach (var workSlot in workSlots)
            {
                foreach (var employee in employees)
                {
                    if (existingWorkslotEmployee.Any(e => e.WorkSlotId == workSlot.Id && e.EmployeeId == employee.Id)) continue;
                    WorkslotEmployee workSlotEmployee = new WorkslotEmployee
                    {
                        Id = Guid.NewGuid(),
                        CheckInTime = null,
                        CheckOutTime = null,
                        EmployeeId = employee.Id,
                        Employee = employee,
                        WorkslotId = workSlot.Id,
                        Workslot = workSlot,
                        AttendanceStatusId = attandance.Id,
                        AttendanceStatus = att,
                        IsDeleted = false
                    };
                    newWorkSlotEmployees.Add(workSlotEmployee);
                }
            }

            // Add to database and save
            _dbContext.WorkslotEmployees.AddRange(newWorkSlotEmployees);
            await _dbContext.SaveChangesAsync();

            return newWorkSlotEmployees.Select(x => new
            {
                workslotEmployeeId = x.Id
            }).ToList();
        }

        private async Task<List<TimeSlotDTO>> GetApprovedOvertimeRequests(Guid employeeId)
        {
            var overtimeRequests = await _dbContext.Requests
                .Include(r => r.RequestOverTime)
                .ThenInclude(rot => rot.WorkingStatus)
                .Where(r => r.EmployeeSendRequestId == employeeId && r.Status == RequestStatus.Approved && r.requestType == RequestType.OverTime)
                .OrderBy(r => r.RequestOverTime.DateOfOverTime)
                .ToListAsync();

            return overtimeRequests.Select(ot => new TimeSlotDTO
            {
                Date = ot.RequestOverTime.DateOfOverTime.ToString("yyyy-MM-dd"),
                StartTime = ot.RequestOverTime.FromHour.ToString("HH:mm"),
                EndTime = ot.RequestOverTime.ToHour.ToString("HH:mm"),
                CheckIn = null, // Overtime doesn't have check-in time
                CheckOut = null, // Overtime doesn't have check-out time
                Duration = (TimeSpan.Parse(ot.RequestOverTime.ToHour.ToString("HH:mm")) - TimeSpan.Parse(ot.RequestOverTime.FromHour.ToString("HH:mm"))).TotalHours.ToString("HH:mm"),
                Status = ot.RequestOverTime.WorkingStatus.Name,
                IsOvertime = true
            }).ToList();
        }

        public class TimeSlotDTO
        {
            public string Date { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string CheckIn { get; set; }
            public string CheckOut { get; set; }
            public string? Duration { get; set; }
            public string Status { get; set; }
            public bool? IsOvertime { get; set; }
        }

        public async Task<object> GetWorkSlotEmployeeByEmployeeId(Guid employeeId)
        {
            // Find the employee by Id
            var employee = await _dbContext.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return "Employee not found";
            }

            // Fetch all WorkSlotEmployee for the employee
            var workSlotEmployees = await _dbContext.WorkslotEmployees
                .Include(we => we.Workslot)
                .Include(we => we.AttendanceStatus)
                .ThenInclude(ast => ast.LeaveType)
                .Include(we => we.AttendanceStatus)
                .ThenInclude(ast => ast.WorkingStatus)
                .Where(we => we.EmployeeId == employeeId)
                .ToListAsync();

            // Group by DateOfSlot
            var groupedWorkSlotEmployees = workSlotEmployees
            .GroupBy(we => we.Workslot.DateOfSlot)
            .OrderBy(g => g.Key)  // Sorting by Date here
            .Select(group => new
            {
                Date = group.Key,
                WorkSlotEmployees = group.ToList()
            }).ToList();

            var result = new List<object>();

            foreach (var group in groupedWorkSlotEmployees)
            {
                var startTime = group.WorkSlotEmployees.FirstOrDefault(we => we.Workslot.IsMorning)?.Workslot.FromHour;
                var endTime = group.WorkSlotEmployees.FirstOrDefault(we => !we.Workslot.IsMorning)?.Workslot.ToHour;
                var checkIn = group.WorkSlotEmployees.FirstOrDefault(we => we.Workslot.IsMorning)?.CheckInTime;
                var checkOut = group.WorkSlotEmployees.FirstOrDefault(we => !we.Workslot.IsMorning)?.CheckOutTime;
                var duration = checkIn != null && checkOut != null ?
                    (TimeSpan.Parse(checkOut) - TimeSpan.Parse(checkIn)).ToString(@"hh\:mm") :
                    null;
                var status = group.WorkSlotEmployees.First().AttendanceStatus.LeaveTypeId.HasValue ?
                    group.WorkSlotEmployees.First().AttendanceStatus.LeaveType.Name :
                    group.WorkSlotEmployees.First().AttendanceStatus.WorkingStatus.Name;

                result.Add(new TimeSlotDTO
                {
                    Date = group.Date.ToString("yyyy-MM-dd"),
                    StartTime = startTime,
                    EndTime = endTime,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    Duration = duration,
                    Status = status,
                    IsOvertime = false
                });
            }

            // end of Time Slot
            var requestOfEmployee = _dbContext.Requests.Include(rq => rq.RequestLeave).Include(rq => rq.RequestWorkTime).Include(rq => rq.RequestOverTime).Where(rq => rq.EmployeeSendRequestId == employeeId);
            var requestLeavePending = requestOfEmployee.Where(rq => rq.requestType == RequestType.Leave).Where(rq => rq.Status == RequestStatus.Pending).Count();
            var requestWorkTimePending = requestOfEmployee.Where(rq => rq.requestType == RequestType.WorkTime).Where(rq => rq.Status == RequestStatus.Pending).Count();
            var requestOverTimePending = requestOfEmployee.Where(rq => rq.requestType == RequestType.OverTime).Where(rq => rq.Status == RequestStatus.Approved).Count();
            // end of request
            int[] monthlyWorkedHours = new int[12];

            workSlotEmployees = workSlotEmployees.Where(we => we.AttendanceStatus.WorkingStatus != null && we.AttendanceStatus.WorkingStatus.Name == "Worked").ToList();
            foreach (var we in workSlotEmployees)
            {
                var startTime = TimeSpan.Parse(we.Workslot.FromHour);
                var endTime = TimeSpan.Parse(we.Workslot.ToHour);
                var duration = (endTime - startTime).TotalHours;

                // Add to the corresponding month
                int month = we.Workslot.DateOfSlot.Month - 1;  // Months are 0-indexed in the array
                monthlyWorkedHours[month] += (int)duration;
            }

            // Prepare the result
            var AllTimeWork = new
            {
                Name = "Worked",
                Data = monthlyWorkedHours
            };

            result.AddRange(await GetApprovedOvertimeRequests(employeeId));

            return new
            {
                requestLeavePending,
                requestOverTimePending,
                requestWorkTimePending,
                AllTimeWork,
                TimeSlot = result,
            };
        }

        public class TimeSheetDTO
        {
            public string? Date { get; set; }
            public string? In { get; set; }
            public string? Out { get; set; }
            public string? Duration { get; set; }
            public bool? isOvertime { get; set; }
            public string? Coefficients { get; set; }

        }

        public async Task<List<TimeSheetDTO>> GetApprovedOvertimeRequestsAsync(Guid employeeId, string startTime, string endTime)
        {
            var startDate = DateTime.ParseExact(startTime, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(endTime, "yyyy/MM/dd", CultureInfo.InvariantCulture);

            // Fetch all approved overtime requests within the date range for the specified employee
            var overtimeRequests = await _dbContext.Requests
                .Include(r => r.RequestOverTime)
                .Where(r => r.EmployeeSendRequestId == employeeId
                            && r.Status == RequestStatus.Approved
                            && r.requestType == RequestType.OverTime
                            && r.RequestOverTime.DateOfOverTime >= startDate
                            && r.RequestOverTime.DateOfOverTime <= endDate)
                .ToListAsync();

            var results = new List<TimeSheetDTO>();
            foreach (var request in overtimeRequests)
            {
                var date = request.RequestOverTime.DateOfOverTime.ToString("yyyy/MM/dd");
                var isHoliday = await IsHoliday(date);
                var isNonWorkingDay = IsNonWorkingDay(request.RequestOverTime.DateOfOverTime, request.EmployeeSendRequest.DepartmentId.Value);
                string checkin = request.RequestOverTime.CheckInTime;
                string checkout = request.RequestOverTime.CheckOutTime;
                string coefficient = isHoliday ? "x3" :
                                     isNonWorkingDay ? "x2" : "x1.5";

                results.Add(new TimeSheetDTO
                {
                    Date = date ?? "N/A",
                    In = checkin ?? "N/A",
                    Out = checkout ?? "N/A",
                    Duration = TimeSpan.TryParse(checkin, out TimeSpan startTimeSpan) && TimeSpan.TryParse(checkout, out TimeSpan endTimeSpan)
               ? (endTimeSpan - startTimeSpan).ToString(@"hh\:mm")
               : "N/A",
                    Coefficients = coefficient ?? "N/A",
                    isOvertime = true
                });
            }

            return results;
        }

        public async Task<List<object>> GetWorkSlotEmployeesByDepartmentId(string? departmentId, string starttime, string endtime)
        {
            // fetch all employees of the department
            var employees = _dbContext.Employees.ToList();
            if (departmentId != null)
            {
                employees = employees.Where(e => e.DepartmentId == Guid.Parse(departmentId))
                .ToList();
            }

            var allEmployeeResults = new List<object>();

            string ConvertHoursToTimeString(double numberOfHours)
            {
                int wholeHours = (int)numberOfHours;
                int minutes = (int)Math.Round((numberOfHours - wholeHours) * 60);
                return $"{wholeHours:D2}:{minutes:D2}";
            }

            foreach (var employee in employees)
            {
                var employeeId = employee.Id;

                var TeamName = employee.DepartmentId != null ? _dbContext.Departments.FirstOrDefault(d => d.Id == employee.DepartmentId).Name : "N/A";

                // Fetch all WorkSlotEmployee for the employee
                var workSlotEmployees = await _dbContext.WorkslotEmployees
                    .Include(we => we.Workslot)
                    .Include(we => we.AttendanceStatus)
                    .Where(we => we.EmployeeId == employeeId)
                    .ToListAsync();

                // Check for overtime
                var requestOfEmployee = _dbContext.Requests.Include(rq => rq.RequestOverTime).Include(r => r.RequestWorkTime)
                    .Where(rq => rq.EmployeeSendRequestId == employeeId);

                var requestOverTimePending = requestOfEmployee
                    .Where(rq => rq.requestType == RequestType.OverTime)
                    .Where(rq => rq.Status == RequestStatus.Approved)
                    .Select(rq => rq.RequestOverTime);
                var totalOvertime = requestOverTimePending.Select(r => r.NumberOfHour).Sum();
                //DateTime dateTime = DateTime.ParseExact(month, "yyyy/MM/dd", CultureInfo.InvariantCulture);

                DateTime startDate = DateTime.ParseExact(starttime, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(endtime, "yyyy/MM/dd", CultureInfo.InvariantCulture);

                // Group by DateOfSlot and sort by Date
                var groupedWorkSlotEmployees = workSlotEmployees
                    .Where(we => we.Workslot.DateOfSlot >= startDate && we.Workslot.DateOfSlot <= endDate)
                    .GroupBy(we => we.Workslot.DateOfSlot)
                    .OrderBy(group => group.Key)
                    .Select(group => new TimeSheetDTO()
                    {
                        Date = group.Key.ToString("yyyy/MM/dd"),
                        In = group.FirstOrDefault(we => we.Workslot.IsMorning)?.CheckInTime ?? "N/A",
                        Out = group.FirstOrDefault(we => !we.Workslot.IsMorning)?.CheckOutTime ?? "N/A",
                        Duration = (group.FirstOrDefault(we => we.Workslot.IsMorning)?.CheckInTime == null || group.FirstOrDefault(we => !we.Workslot.IsMorning)?.CheckOutTime == null) ?
                        "N/A" :
                   (TimeSpan.ParseExact(group.FirstOrDefault(we => !we.Workslot.IsMorning)?.CheckOutTime ?? "00:00", @"hh\:mm", CultureInfo.InvariantCulture) -
                    TimeSpan.ParseExact(group.FirstOrDefault(we => we.Workslot.IsMorning)?.CheckInTime ?? "00:00", @"hh\:mm", CultureInfo.InvariantCulture)).ToString(@"hh\:mm"),
                        Coefficients = "N/A",
                        isOvertime = false
                    }).ToList();
                groupedWorkSlotEmployees.AddRange(await GetApprovedOvertimeRequestsAsync(employeeId, starttime, endtime));

                groupedWorkSlotEmployees = groupedWorkSlotEmployees.OrderBy(item => DateTime.ParseExact(item.Date, "yyyy/MM/dd", CultureInfo.InvariantCulture))  // Sort by Date
                .ToList();

                double totalWorkedHours = groupedWorkSlotEmployees
                    .Where(item => item.Duration != "N/A")
                    .Select(item => TimeSpan.ParseExact(item.Duration, @"hh\:mm", CultureInfo.InvariantCulture).TotalHours)
                    .Sum();

                allEmployeeResults.Add(new
                {
                    Name = employee.FirstName + " " + employee.LastName,
                    Working = groupedWorkSlotEmployees,
                    TotalOvertime = ConvertHoursToTimeString(totalOvertime),  // converted to "HH:mm"
                    TotalWorkedHours = ConvertHoursToTimeString(totalWorkedHours),
                    TeamName = TeamName// sum of Duration converted to "HH:mm"
                });
            }

            return allEmployeeResults;
        }

        public async Task<bool> IsHoliday(string dateString)
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

        private bool IsNonWorkingDay(DateTime date, Guid departmentId)
        {
            var department = _dbContext.Departments
                .Include(d => d.WorkTrackSetting)
                .ThenInclude(wts => wts.WorkDateSetting)
                .FirstOrDefault(d => d.Id == departmentId);

            if (department == null)
            {
                throw new Exception("Team not existing");
            }

            var workTrackSetting = department.WorkTrackSetting;

            // Deserialize the WorkDateSetting to know which days are workdays
            DateStatusDTO workDays = JsonSerializer.Deserialize<DateStatusDTO>(workTrackSetting.WorkDateSetting.DateStatus);

            string dayOfWeek = date.DayOfWeek.ToString();
            bool isWorkDay = (bool)typeof(DateStatusDTO).GetProperty(dayOfWeek).GetValue(workDays);

            return !isWorkDay;
        }

        private async Task<string> GetCoefficient(DateTime date, bool isOvertime, Guid departmentId)
        {
            if (!isOvertime)
                return "x1"; // Normal working hours have no multiplier

            bool holiday = await IsHoliday(date.ToString("yyyy/MM/dd"));
            if (holiday)
                return coefficients["holiday"];

            bool nonWorkingDay = IsNonWorkingDay(date, departmentId);
            if (nonWorkingDay)
                return coefficients["nonWorkingDay"];

            return coefficients["normalDay"];
        }

        //    public async Task<List<object>> GetWorkSlotEmployeesByDepartmentId(Guid? departmentId, string startTime, string endTime)
        //    {
        //        var startDate = DateTime.ParseExact(startTime, "yyyy/MM/dd", CultureInfo.InvariantCulture);
        //        var endDate = DateTime.ParseExact(endTime, "yyyy/MM/dd", CultureInfo.InvariantCulture);

        //        // Filter employees based on departmentId if provided
        //        var employeesQuery = _dbContext.Employees.AsQueryable();
        //        if (departmentId.HasValue)
        //        {
        //            employeesQuery = employeesQuery.Where(e => e.DepartmentId == departmentId.Value);
        //        }

        //        var employees = await employeesQuery.ToListAsync();
        //        var allEmployeeResults = new List<object>();

        //        // Fetch the coefficients from Firebase or a similar approach
        //        var coefficients = new Dictionary<string, string> {
        //    { "normalDay", "x1.5" },
        //    { "nonWorkingDay", "x2" },
        //    { "holiday", "x3" }
        //};

        //        foreach (var employee in employees)
        //        {
        //            var workSlotEmployees = await _dbContext.WorkslotEmployees
        //                .Include(we => we.Workslot)
        //                .Include(we => we.AttendanceStatus)
        //                .Where(we => we.EmployeeId == employee.Id && we.Workslot.DateOfSlot >= startDate && we.Workslot.DateOfSlot <= endDate)
        //                .ToListAsync();

        //            var overtimeRequests = await _dbContext.Requests
        //                .Include(r => r.RequestOverTime)
        //                .Where(r => r.EmployeeSendRequestId == employee.Id && r.RequestOverTime.DateOfOverTime >= startDate && r.RequestOverTime.DateOfOverTime <= endDate && r.Status == RequestStatus.Approved)
        //                .ToListAsync();

        //            var workingDetails = new List<object>();

        //            foreach (var workSlotEmployee in workSlotEmployees)
        //            {
        //                var dateStr = workSlotEmployee.Workslot.DateOfSlot.ToString("yyyy/MM/dd");
        //                var isHoliday = await IsHoliday(dateStr);
        //                var isNonWorkingDay = IsNonWorkingDay(workSlotEmployee.Workslot.DateOfSlot, employee.DepartmentId.Value);

        //                string coefficient = isHoliday ? coefficients["holiday"] : (isNonWorkingDay ? coefficients["nonWorkingDay"] : coefficients["normalDay"]);

        //                workingDetails.Add(new
        //                {
        //                    date = dateStr,
        //                    inn = workSlotEmployee.CheckInTime ?? "N/A",
        //                    outt = workSlotEmployee.CheckOutTime ?? "N/A",
        //                    duration = "N/A", // Calculate duration if needed
        //                    COEFFICIENTS = coefficient,
        //                    isOvertime = false
        //                });
        //            }

        //            foreach (var overtimeRequest in overtimeRequests)
        //            {
        //                var dateStr = overtimeRequest.RequestOverTime.DateOfOverTime.ToString("yyyy/MM/dd");
        //                var isHoliday = await IsHoliday(dateStr);
        //                var isNonWorkingDay = IsNonWorkingDay(overtimeRequest.RequestOverTime.DateOfOverTime, employee.DepartmentId.Value);

        //                string coefficient = isHoliday ? coefficients["holiday"] : (isNonWorkingDay ? coefficients["nonWorkingDay"] : coefficients["normalDay"]);

        //                workingDetails.Add(new
        //                {
        //                    date = dateStr,
        //            inn = overtimeRequest.RequestOverTime.FromHour,
        //            outt = overtimeRequest.RequestOverTime.ToHour,
        //                    duration = (overtimeRequest.RequestOverTime.ToHour - overtimeRequest.RequestOverTime.FromHour).ToString(@"hh\:mm"),
        //                    COEFFICIENTS = coefficient,
        //                    isOvertime = true
        //                });
        //            }

        //            allEmployeeResults.Add(new
        //            {
        //                Name = employee.FirstName + " " + employee.LastName,
        //                Working = workingDetails,
        //                TeamName = employee.Department != null ? employee.Department.Name : "No Team"
        //            });
        //        }

        //        return allEmployeeResults;
        //    }

        public static string ConvertHoursToTimeString(double numberOfHours)
        {
            // Extract whole hours and fractional hours
            int wholeHours = (int)numberOfHours;
            double fractionalHours = numberOfHours - wholeHours;

            // Convert fractional hours to minutes
            int minutes = (int)Math.Round(fractionalHours * 60);

            // Format into a string
            return $"{wholeHours:D2}:{minutes:D2}";
        }

        public class TimeSlotDto
        {
            public string Date { get; set; }
            public string Status { get; set; }
        }

        public async Task<List<TimeSlotDto>> GetTimeSlotsByEmployeeId(Guid employeeId)
        {
            // Find the employee by Id
            var employee = await _dbContext.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return null; // Or throw an exception
            }

            // Fetch all WorkSlotEmployee for the employee
            var workSlotEmployees = await _dbContext.WorkslotEmployees
                .Include(we => we.Workslot)
                .Include(we => we.AttendanceStatus)
                .ThenInclude(ast => ast.LeaveType)
                .Include(we => we.AttendanceStatus)
                .ThenInclude(ast => ast.WorkingStatus)
                .Where(we => we.EmployeeId == employeeId)
                .ToListAsync();

            // Group by DateOfSlot and prepare TimeSlot data
            var groupedWorkSlotEmployees = workSlotEmployees
                .GroupBy(we => we.Workslot.DateOfSlot)
                .OrderBy(g => g.Key)
                .Select(group => new TimeSlotDto
                {
                    Date = group.Key.ToString("yyyy-MM-dd"),
                    Status = group.First().AttendanceStatus.LeaveTypeId.HasValue ?
                             group.First().AttendanceStatus.LeaveType.Name :
                             group.First().AttendanceStatus.WorkingStatus.Name
                }).ToList();

            return groupedWorkSlotEmployees;
        }

        public static DateTime GetOneHourSoonerDateTime(string inputTimeStr)
        {
            DateTime inputTime = DateTime.ParseExact(inputTimeStr, "HH:mm", CultureInfo.InvariantCulture);
            DateTime oneHourSooner = inputTime.AddHours(-1);
            return oneHourSooner;
        }

        public static DateTime GetThirtyMinutesSoonerDateTime(string inputTimeStr)
        {
            DateTime inputTime = DateTime.ParseExact(inputTimeStr, "HH:mm", CultureInfo.InvariantCulture);
            DateTime oneHourSooner = inputTime.AddMinutes(-30);
            return oneHourSooner;
        }

        public static DateTime GetThirtyMinutesLaterDateTime(string inputTimeStr)
        {
            DateTime inputTime = DateTime.ParseExact(inputTimeStr, "HH:mm", CultureInfo.InvariantCulture);
            DateTime thirtyMinutesLater = inputTime.AddMinutes(30);
            return thirtyMinutesLater;
        }

        public static DateTime GetOneHourLaterDateTime(string inputTimeStr)
        {
            DateTime inputTime = DateTime.ParseExact(inputTimeStr, "HH:mm", CultureInfo.InvariantCulture);
            DateTime thirtyMinutesLater = inputTime.AddHours(1);
            return thirtyMinutesLater;
        }

        public async Task<object> CheckInWorkslotEmployee(Guid employeeId, DateTime? currentTime)
        {
            // Step 1: Retrieve relevant WorkslotEmployee record
            if (currentTime == null) currentTime = DateTime.Now;
            var currentDate = currentTime.Value.Date;


            var workslotEmployees = await _dbContext.WorkslotEmployees
    .Include(we => we.Workslot)
    .Include(we => we.AttendanceStatus)
    .Where(we => we.EmployeeId == employeeId && we.Workslot.DateOfSlot.Date == currentDate)
    .ToListAsync();

            //var workslotEmployee = workslotEmployees.Where(we => we.Workslot.IsMorning)
            //    .FirstOrDefault(we => GetOneHourSoonerDateTime(we.Workslot.FromHour) <= currentTime &&
            //                          GetThirtyMinutesLaterDateTime(we.Workslot.FromHour) >= currentTime);

            var workslotEmployee = workslotEmployees.Where(we => we.Workslot.IsMorning)
                .FirstOrDefault(we => we.Workslot.DateOfSlot.Date == currentDate);

            if (workslotEmployee == null)
            {
                return new { message = "No eligible Workslot for check-in found." };
            }

            // Step 2: Update CheckIn time
            workslotEmployee.CheckInTime = currentTime?.ToString("HH:mm");
            workslotEmployee.CheckOutTime = "12:00";

            // Step 3: Update AttendanceStatus
            var newAttendanceStatus = await _dbContext.AttendanceStatuses
                                                      .Include(att => att.WorkingStatus)
                                                      .FirstOrDefaultAsync(att => att.WorkingStatus != null && att.WorkingStatus.Name == "Working");

            if (newAttendanceStatus == null)
            {
                return new { message = "Attendance status for the WorkingStatus 'Worked' not found." };
            }

            workslotEmployee.AttendanceStatus = newAttendanceStatus;
            workslotEmployee.AttendanceStatusId = newAttendanceStatus.Id;
            //var evenning = workslotEmployees.FirstOrDefault(w => w.Workslot.IsMorning == false);
            //if (evenning != null)
            //{
            //    evenning.AttendanceStatus = newAttendanceStatus;
            //    evenning.AttendanceStatusId = newAttendanceStatus.Id;
            //}

            // Step 4: Save changes to the database
            await _dbContext.SaveChangesAsync();

            return new { message = "Successfully checked in." };
        }

        //public async Task<object> CheckOutWorkslotEmployee(Guid employeeId, DateTime? currentTime)
        //{
        //    // Step 1: Retrieve relevant WorkslotEmployee record
        //    if (currentTime == null) currentTime = DateTime.Now;
        //    var currentDate = currentTime.Value.Date;

        //    var workslotEmployees = await _dbContext.WorkslotEmployees
        //        .Include(we => we.Workslot)
        //        .Include(we => we.AttendanceStatus)
        //        .Where(we => we.EmployeeId == employeeId && we.Workslot.DateOfSlot.Date == currentDate)
        //        .ToListAsync();

        //    var workslotEmployee = workslotEmployees.Where(w => w.Workslot.IsMorning == false)
        //        .FirstOrDefault(we => we.Workslot.DateOfSlot.Date == currentDate);

        //    if (workslotEmployee == null)
        //    {
        //        return new { message = "No eligible Workslot for check-out found." };
        //    }

        //    // Step 2: Update CheckOut time
        //    //workslotEmployee.CheckInTime = w
        //    workslotEmployee.CheckOutTime = currentTime?.ToString("HH:mm");

        //    // Step 3: Update AttendanceStatus
        //    var newAttendanceStatus = await _dbContext.AttendanceStatuses
        //                                              .Include(att => att.WorkingStatus)
        //                                              .FirstOrDefaultAsync(att => att.WorkingStatus != null && att.WorkingStatus.Name == "Worked");

        //    if (newAttendanceStatus == null)
        //    {
        //        return new { message = "Attendance status for the WorkingStatus 'Worked' not found." };
        //    }

        //    workslotEmployee.AttendanceStatus = newAttendanceStatus;
        //    workslotEmployee.AttendanceStatusId = newAttendanceStatus.Id;
        //    var morning = workslotEmployees.FirstOrDefault(w => w.Workslot.IsMorning);
        //    if (morning != null)
        //    {
        //        morning.AttendanceStatus = newAttendanceStatus;
        //        morning.AttendanceStatusId = newAttendanceStatus.Id;
        //    }

        //    // Step 4: Save changes to the database
        //    await _dbContext.SaveChangesAsync();

        //    return new { message = "Successfully checked out." };
        //}

        public async Task<object> CheckOutWorkslotEmployee(Guid employeeId, DateTime? currentTime)
        {
            // Step 1: Retrieve relevant WorkslotEmployee record
            if (currentTime == null) currentTime = DateTime.Now;
            var currentDate = currentTime.Value.Date;

            var workslotEmployees = await _dbContext.WorkslotEmployees
                .Include(we => we.Workslot)
                .Include(we => we.AttendanceStatus)
                .Where(we => we.EmployeeId == employeeId && we.Workslot.DateOfSlot.Date == currentDate)
                .ToListAsync();

            var eveningSlot = workslotEmployees.FirstOrDefault(w => !w.Workslot.IsMorning);
            var morningSlot = workslotEmployees.FirstOrDefault(w => w.Workslot.IsMorning);

            if (eveningSlot == null)
            {
                throw new Exception("No eligible Workslot for check-out found.");
            }

            // Step 2: Update CheckOut time
            eveningSlot.CheckOutTime = currentTime?.ToString("HH:mm");
            eveningSlot.CheckInTime = "13:00";

            // Step 3: Update AttendanceStatus
            double duration = 0;
            if (morningSlot != null && !string.IsNullOrEmpty(morningSlot.CheckInTime))
            {
                duration = DateTime.ParseExact(eveningSlot.CheckOutTime, "HH:mm", CultureInfo.InvariantCulture)
                          .Subtract(DateTime.ParseExact(morningSlot.CheckInTime, "HH:mm", CultureInfo.InvariantCulture)).TotalHours;
            }

            string statusName = duration < 9 ? "Lack of Time" : "Worked";
            var newAttendanceStatus = await _dbContext.AttendanceStatuses
                                                      .Include(att => att.WorkingStatus)
                                                      .FirstOrDefaultAsync(att => att.WorkingStatus != null && att.WorkingStatus.Name == statusName);

            if (newAttendanceStatus == null)
            {
                throw new Exception($"Attendance status for the WorkingStatus '{statusName}' not found.");
            }

            eveningSlot.AttendanceStatus = newAttendanceStatus;
            eveningSlot.AttendanceStatusId = newAttendanceStatus.Id;
            if (morningSlot != null)
            {
                morningSlot.AttendanceStatus = newAttendanceStatus;
                morningSlot.AttendanceStatusId = newAttendanceStatus.Id;
            }

            // Step 4: Save changes to the database
            await _dbContext.SaveChangesAsync();

            return new { message = "Successfully checked out." };
        }


        // Generate fake checkin checkout data
        public async Task<object> SimulateCheckInOutForDepartment(Guid departmentId, string startDateStr, string endDateStr)
        {
            DateTime startDate;
            DateTime endDate;

            // Attempt to parse the start and end date strings
            try
            {
                startDate = DateTime.ParseExact(startDateStr, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(endDateStr, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                throw new Exception($"Invalid date format: {ex.Message}");
            }

            if (startDate > endDate)
            {
                throw new Exception("Start date must be earlier than or equal to end date.");
            }

            var departmentEmployees = await _dbContext.Employees
                .Where(e => e.DepartmentId == departmentId && !e.IsDeleted)
                .ToListAsync();

            Random rand = new Random();
            var listEmployeeCheckin = new List<object>();

            foreach (var employee in departmentEmployees)
            {
                // Retrieve approved leave days for this employee
                var approvedLeaveDates = await GetApprovedLeaveDaysByEmployeeIdAsync(employee.Id);
                var workslotCheckin = new List<object>();
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    // Check if this date is an approved leave date
                    if (approvedLeaveDates.Contains(date.Date))
                        continue;  // Skip check-in/check-out processing for this date

                    var workslots = await _dbContext.WorkslotEmployees
                        .Include(wse => wse.Workslot)
                        .Where(wse => wse.EmployeeId == employee.Id && wse.Workslot.DateOfSlot.Date == date)
                        .OrderBy(wse => !wse.Workslot.IsMorning) // This ensures that morning slots (true) come before afternoon slots (false)
                        .ToListAsync();

                    foreach (var slot in workslots)
                    {
                        if (slot.Workslot.IsMorning)
                        {
                            var checkInTime = GetRandomCheckInTime(slot.Workslot.FromHour, rand);
                            await CheckInWorkslotEmployee(employee.Id, ConvertToDateTime(date, checkInTime)); // Random morning check-in time
                            workslotCheckin.Add(new { slotId = slot.WorkslotId });
                        }
                        else
                        {
                            var checkOutTime = GetRandomCheckOutTime(slot.Workslot.ToHour, rand);
                            await CheckOutWorkslotEmployee(employee.Id, ConvertToDateTime(date, checkOutTime)); // Random afternoon check-out time
                            workslotCheckin.Add(new { slotId = slot.WorkslotId });
                        }
                    }
                }

                listEmployeeCheckin.Add(new { CheckInedEmployeeId = employee.Id, workslotCheckin });
            }

            return new { startDate = startDate.ToString("yyyy/MM/dd"),
            endDate = endDate.ToString("yyyy/MM/dd"),
            listEmployeeCheckin
            };
        }

        private async Task<HashSet<DateTime>> GetApprovedLeaveDaysByEmployeeIdAsync(Guid employeeId)
        {
            var approvedLeaveDates = new HashSet<DateTime>();

            var approvedLeaveRequests = await _dbContext.Requests
                .Include(r => r.RequestLeave)
                    .ThenInclude(rl => rl.WorkslotEmployees)
                        .ThenInclude(we => we.Workslot)
                .Where(r => r.EmployeeSendRequestId == employeeId && r.Status == RequestStatus.Approved && r.requestType == RequestType.Leave)
                .ToListAsync();

            foreach (var request in approvedLeaveRequests)
            {
                var leaveDates = request.RequestLeave.WorkslotEmployees
                    .Where(we => !we.IsDeleted)
                    .Select(we => we.Workslot.DateOfSlot)
                    .Distinct();

                foreach (var date in leaveDates)
                {
                    approvedLeaveDates.Add(date.Date);
                }
            }

            return approvedLeaveDates;
        }

        private string GetRandomCheckInTime(string baseTime, Random rand)
        {
            var time = DateTime.ParseExact(baseTime, "HH:mm", CultureInfo.InvariantCulture);
            // Random time up to 30 minutes before (-30) and up to 30 minutes after (+30)
            var minutesToAdd = rand.Next(-30, 31); // Include +30 and -30 minutes
            return time.AddMinutes(minutesToAdd).ToString("HH:mm");
        }

        private string GetRandomCheckOutTime(string baseTime, Random rand)
        {
            var time = DateTime.ParseExact(baseTime, "HH:mm", CultureInfo.InvariantCulture);
            // Random time up to 30 minutes before and up to 30 minutes after
            var minutesToAdd = rand.Next(-30, 31); // Same adjustment as for check-in
            return time.AddMinutes(minutesToAdd).ToString("HH:mm");
        }

        private DateTime ConvertToDateTime(DateTime date, string time)
        {
            return DateTime.ParseExact($"{date.ToString("yyyy/MM/dd")}-{time}", "yyyy/MM/dd-HH:mm", CultureInfo.InvariantCulture);
        }
        // End of generate checkin checkout data


        public async Task<object> CheckInOutForPeriod(Guid departmentId, DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                return new { message = "Start date must be earlier than or equal to end date." };
            }
            var messages = new List<string>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Fetch work slots for the given date
                var listEmp = await _departmentRepository.GetEmployeesByDepartmentIdAsync(departmentId);
                var listEmpId = listEmp.Select(e => e.Id).ToList();
                foreach (var employeeId in listEmpId)
                {
                    var workslotEmployees = await _dbContext.WorkslotEmployees
                    .Include(we => we.Workslot)
                    .Where(we => we.EmployeeId == employeeId && we.Workslot.DateOfSlot.Date == date)
                    .ToListAsync();

                    var morningSlot = workslotEmployees.FirstOrDefault(w => w.Workslot.IsMorning);
                    var afternoonSlot = workslotEmployees.FirstOrDefault(w => !w.Workslot.IsMorning);

                    if (morningSlot != null)
                    {
                        DateTime checkInBaseTime = ConvertToDateTime(date, morningSlot.Workslot.FromHour);
                        var checkInTime = GenerateRandomTimeAround(checkInBaseTime);
                        var checkInResult = await CheckInWorkslotEmployee((Guid)employeeId, checkInTime);
                    }

                    if (afternoonSlot != null)
                    {
                        DateTime checkOutBaseTime = ConvertToDateTime(date, afternoonSlot.Workslot.ToHour);
                        var checkOutTime = GenerateRandomTimeAround(checkOutBaseTime);
                        var checkOutResult = await CheckOutWorkslotEmployee((Guid)employeeId, checkOutTime);
                    }
                }
            }

            return new { messages };
        }

        private DateTime ConvertToDateTime2(DateTime date, string time)
        {
            var timeParts = time.Split(':');
            int hour = int.Parse(timeParts[0]);
            int minute = int.Parse(timeParts[1]);
            return new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);
        }

        private DateTime GenerateRandomTimeAround(DateTime targetTime)
        {
            // Generate a random number between -60 to 60 minutes
            var random = new Random();
            int randomMinutes = random.Next(-60, 61);
            return targetTime.AddMinutes(randomMinutes);
        }

        public async Task<string> ExportWorkSlotEmployeeReport(Guid departmentId)
        {
            // Initialize EPPlus Excel package
            using (var package = new ExcelPackage())
            {
                // Add a worksheet
                var worksheet = package.Workbook.Worksheets.Add("WorkSlotEmployeeReport");

                // Define the status short names
                var statusShortNames = new Dictionary<string, string>
        {
            { "Not Work Yet", "NWD" },
            { "Worked", "WD" },
                    {"Annual Leave", "AL" },
                    {"Maternity Leave", "ML" },
                    {"Sick Leave", "SL" },
                    {"Paternity Leave", "PL" },
                    {"Unpaid Leave", "UL" },
                    {"Study Leave", "STL" },
                    {"WFH", "WFH" },
                    {"Absent", "AS" },
                    {"Training", "TN" },
                    {"Working", "WK" },
                    {"Remote Working", "RW" },
                    {"Lack of Time", "LOT" }

        };


                // Add the status key to the first and second rows
                worksheet.Cells[1, 1].Value = "Not-Working Date / NWD";
                worksheet.Cells[2, 1].Value = "Working Date / WD";
                worksheet.Cells[3, 1].Value = "Annual Leave / AL";
                worksheet.Cells[4, 1].Value = "Maternity Leave / ML";
                worksheet.Cells[5, 1].Value = "Sick Leave / SL";
                worksheet.Cells[6, 1].Value = "Paternity Leave / PL";
                worksheet.Cells[7, 1].Value = "Unpaid Leave / UL";
                worksheet.Cells[8, 1].Value = "Study Leave / STL";
                worksheet.Cells[9, 1].Value = "Lack of Time / LOT";
                worksheet.Cells[10, 1].Value = "Absent / AS";
                worksheet.Cells[11, 1].Value = "Training / TN";
                worksheet.Cells[12, 1].Value = "Working / WK";
                worksheet.Cells[13, 1].Value = "Remote Working / RW";


                // Fetch all employees in the department
                var employees = await _dbContext.Employees.Where(e => e.DepartmentId == departmentId).ToListAsync();

                // Initialize list to store the TimeSlot data for all employees
                var allTimeSlots = new List<dynamic>();

                foreach (var employee in employees)
                {
                    // Fetch the TimeSlot data using your existing method
                    var workSlotData = await GetTimeSlotsByEmployeeId(employee.Id);
                    var timeSlots = workSlotData;

                    // Add this employee's TimeSlot data to the list
                    foreach (var timeSlot in timeSlots)
                    {
                        allTimeSlots.Add(new { EmployeeName = employee.FirstName + " " + employee.LastName, Date = timeSlot.Date, Status = timeSlot.Status });
                    }
                }

                // Headers for the Excel file
                worksheet.Cells[15, 1].Value = "Employee Name";
                worksheet.Cells[15, 1].AutoFitColumns();  // Auto-resize

                var distinctDates = allTimeSlots.Select(d => d.Date).Distinct().OrderBy(d => d).ToList();
                for (int i = 0; i < distinctDates.Count; i++)
                {
                    worksheet.Cells[15, i + 2].Value = distinctDates[i];
                    worksheet.Cells[15, i + 2].AutoFitColumns();  // Auto-resize for date columns
                }

                // Set Border Style for the entire range of data
                var endColumn = distinctDates.Count + 1;
                var endRow = employees.Count() + 15;
                using (var range = worksheet.Cells[15, 1, endRow, endColumn])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                // Populate the data
                var groupedData = allTimeSlots.GroupBy(d => d.EmployeeName).ToList();
                var statusColors = new Dictionary<string, Color>
{
    { "NWD", Color.Orange },
    { "WD", Color.Green },
    { "AL", Color.Blue },
    { "ML", Color.Purple },
    { "SL", Color.Red },
    { "PL", Color.Pink },
    { "UL", Color.Brown },
    { "STL", Color.Magenta },
    { "WFH", Color.Gray },
    { "AS", Color.Black },
    { "TN", Color.Yellow },
    { "WK", Color.Cyan },
    { "RW", Color.DarkGreen }
}; int row = 16;
                foreach (var group in groupedData)
                {
                    worksheet.Cells[row, 1].Value = group.Key; // Employee Name
                    worksheet.Cells[row, 1].AutoFitColumns();  // Auto-resize

                    foreach (var record in group)
                    {
                        int col = distinctDates.IndexOf(record.Date) + 2;
                        worksheet.Cells[row, col].Value = statusShortNames[record.Status];

                        // Set background color
                        if (statusColors.ContainsKey(statusShortNames[record.Status]))
                        {
                            worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(statusColors[statusShortNames[record.Status]]);
                        }

                        // Auto-resize the column
                        worksheet.Cells[row, col].AutoFitColumns();
                    }
                    row++;
                }

                // Save the Excel package to a MemoryStream
                var stream = new MemoryStream();
                package.SaveAs(stream);

                // Save the stream to your desired location
                var filePath = "./WorkSlotEmployeeReport.xlsx";
                await File.WriteAllBytesAsync(filePath, stream.ToArray());

                return filePath;
            }
        }

        public async Task<object> GetWorkSlotEmployeeByEmployeeIdForToday(Guid employeeId)
        {
            var today = DateTime.Now.Date;
            //var today = DateTime.ParseExact("2023/09/29", "yyyy/MM/dd", CultureInfo.InvariantCulture);

            // Find the employee by Id
            var employee = await _dbContext.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return "Employee not found";
            }

            // Fetch all WorkSlotEmployee for the employee for today's date
            var workSlotEmployees = await _dbContext.WorkslotEmployees
                .Include(we => we.Workslot)
                .Include(we => we.AttendanceStatus)
                .ThenInclude(ast => ast.LeaveType)
                .Include(we => we.AttendanceStatus)
                .ThenInclude(ast => ast.WorkingStatus)
                .Where(we => we.EmployeeId == employeeId && we.Workslot.DateOfSlot == today)
                .ToListAsync();

            // Extract morning and evening work slot details
            var morningSlot = workSlotEmployees.FirstOrDefault(we => we.Workslot.IsMorning);
            var eveningSlot = workSlotEmployees.FirstOrDefault(we => !we.Workslot.IsMorning);

            var startTime = morningSlot?.Workslot.FromHour;
            var endTime = eveningSlot?.Workslot.ToHour;
            var checkIn = morningSlot?.CheckInTime;
            var checkOut = eveningSlot?.CheckOutTime;
            var duration = startTime != null && endTime != null ?
                (TimeSpan.Parse(endTime) - TimeSpan.Parse(startTime)).ToString(@"hh\:mm") :
                null;
            var status = (bool)(morningSlot?.AttendanceStatus.LeaveTypeId.HasValue) ?
                morningSlot.AttendanceStatus.LeaveType.Name :
                morningSlot?.AttendanceStatus.WorkingStatus.Name;

            var result = new
            {
                Date = today.ToString("yyyy-MM-dd"),
                StartTime = startTime,
                EndTime = endTime,
                CheckIn = checkIn,
                CheckOut = checkOut,
                Duration = duration,
                Status = status
            };

            return result;
        }


    }
}
