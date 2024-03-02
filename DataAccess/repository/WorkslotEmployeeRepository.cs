using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Drawing;
using System.Globalization;

namespace DataAccess.Repository
{
    public class WorkslotEmployeeRepository : Repository<WorkslotEmployee>, IWorkslotEmployeeRepository
    {
        private readonly MyDbContext _dbContext;
        private readonly IAttendanceStatusRepository _attendanceStatusRepository;
        private readonly IDepartmentRepository _departmentRepository;

        public WorkslotEmployeeRepository(MyDbContext context, IAttendanceStatusRepository attendanceStatusRepository, IDepartmentRepository departmentRepository) : base(context)
        {
            // You can add more specific methods here if needed
            _dbContext = context;
            _attendanceStatusRepository = attendanceStatusRepository;
            _departmentRepository = departmentRepository;
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

            return newWorkSlotEmployees.Select(x => new {
                workslotEmployeeId = x.Id
            }).ToList();
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

                result.Add(new
                {
                    Date = group.Date.ToString("yyyy-MM-dd"),
                    StartTime = startTime,
                    EndTime = endTime,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    Duration = duration,
                    Status = status
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

            //

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
            public string? OverTime { get; set; }
        }

        public async Task<object> GetWorkSlotEmployeesByDepartmentId(Guid departmentId, string startTime, string endTime)
        {
            // Fetch all employees of the department
            var employees = _dbContext.Employees
                .Where(e => e.DepartmentId == departmentId)
                .ToList();

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

                DateTime startDate = DateTime.ParseExact(startTime, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(endTime, "yyyy/MM/dd", CultureInfo.InvariantCulture);

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
                        OverTime = requestOverTimePending.FirstOrDefault(r => r.DateOfOverTime == group.Key) != null ?
                            ConvertHoursToTimeString(requestOverTimePending.FirstOrDefault(r => r.DateOfOverTime == group.Key).NumberOfHour) : "00:00"
                    })
                .OrderBy(item => DateTime.ParseExact(item.Date, "yyyy/MM/dd", CultureInfo.InvariantCulture))  // Sort by Date
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
                    TotalWorkedHours = ConvertHoursToTimeString(totalWorkedHours)  // sum of Duration converted to "HH:mm"
                });
            }

            return allEmployeeResults;
        }

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
            var evenning = workslotEmployees.FirstOrDefault(w => w.Workslot.IsMorning == false);
            if (evenning != null)
            {
                evenning.AttendanceStatus = newAttendanceStatus;
                evenning.AttendanceStatusId = newAttendanceStatus.Id;
            }

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
                return new { message = "No eligible Workslot for check-out found." };
            }

            // Step 2: Update CheckOut time
            eveningSlot.CheckOutTime = currentTime?.ToString("HH:mm");

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
                return new { message = $"Attendance status for the WorkingStatus '{statusName}' not found." };
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

        private DateTime ConvertToDateTime(DateTime date, string time)
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
