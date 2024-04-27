using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text;
using static DataAccess.Repository.WorkslotEmployeeRepository;

namespace DataAccess.Repository
{
    public class RequestWorkTimeRepository : Repository<RequestWorkTime>, IRequestWorkTimeRepository
    {
        private readonly MyDbContext _dbContext;

        public RequestWorkTimeRepository(MyDbContext context) : base(context)
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

        public object GetRequestWorkTimeOfEmployeeById(Guid employeeId)
        {

            var employee = _dbContext.Employees.Where(e => e.IsDeleted == false && e.Id == employeeId).FirstOrDefault();
            var result = new List<RequestWorkTimeDTO>();
            var list = _dbContext.Requests.Include(r => r.RequestWorkTime).ThenInclude(rl => rl.WorkslotEmployee).ThenInclude(we => we.Workslot).Where(r => r.IsDeleted == false)
                 .Where(r => r.EmployeeSendRequestId == employeeId && r.requestType == RequestType.WorkTime)
                 .ToList();
            list.ForEach(r =>
            {
                var morningSlot = _dbContext.WorkslotEmployees.Include(we => we.Workslot).FirstOrDefault(w => w.Id == r.RequestWorkTime.WorkslotEmployeeMorningId);
                result.Add(new RequestWorkTimeDTO()
                {
                    Id = r.Id,
                    Name = r.RequestWorkTime.Name,
                    employeeName = employee.FirstName + " " + employee.LastName,
                    employeeNumber = employee.EmployeeNumber,
                    RealHourStart = r.RequestWorkTime.RealHourStart,
                    RealHourEnd = r.RequestWorkTime.RealHourEnd,
                    NumberOfComeLateHour = r.RequestWorkTime.NumberOfComeLateHour,
                    NumberOfLeaveEarlyHour = r.RequestWorkTime.NumberOfComeLateHour,
                    DateOfWorkTime = r.RequestWorkTime.DateOfSlot?.ToString("yyyy/MM/dd"),
                    status = (int)r.Status,
                    statusName = r.Status.ToString(),
                    SlotStart = morningSlot.Workslot?.FromHour,
                    SlotEnd = r.RequestWorkTime?.WorkslotEmployee?.Workslot?.ToHour,
                    CheckInTime = morningSlot?.CheckInTime,
                    CheckOutTime = r.RequestWorkTime?.WorkslotEmployee?.CheckInTime,
                    reason = r.Reason,
                    reasonReject = r.Message,
                    linkFile = r.PathAttachmentFile,
                    WorkslotEmployeeId = r.RequestWorkTime.WorkslotEmployeeId,
                });
            });

            return result;
        }

        public async Task<object> CreateRequestWorkTime(RequestWorkTimeDTO dto, Guid employeeId)
        {
            // Check for null or invalid DTO fields
            if (dto.WorkslotEmployeeId == null)
            {
                throw new Exception("Lack of required fields: RealHourStart, RealHourEnd, Name, reason");
            }
            var workslotEmployee = _dbContext.WorkslotEmployees.Include(we => we.Workslot).FirstOrDefault(we => we.Id == dto.WorkslotEmployeeId && !we.Workslot.IsMorning);
            // Initialize new RequestWorkTime object
            RequestWorkTime newRequestWorkTime = new RequestWorkTime()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? "",
                RealHourStart = dto.RealHourStart,
                RealHourEnd = dto.RealHourEnd,
                NumberOfComeLateHour = dto.NumberOfComeLateHour,
                NumberOfLeaveEarlyHour = dto.NumberOfLeaveEarlyHour,
                DateOfSlot = workslotEmployee.Workslot.DateOfSlot,
                WorkslotEmployeeId = dto.WorkslotEmployeeId,
                WorkslotEmployee = workslotEmployee,
                WorkslotEmployeeMorningId = dto.workslotEmployeeMorningId,
                attendanceStatusMorningId = dto.attendanceStatusMorningId,
                attendanceStatusAfternoonId = dto.attendanceStatusAfternoonId,
                IsDeleted = false  // Set the soft delete flag to false
            };

            await _dbContext.RequestWorkTimes.AddAsync(newRequestWorkTime);

            Employee employeeSendRequest = _dbContext.Employees.FirstOrDefault(e => e.Id == employeeId);
            if (employeeSendRequest == null)
            {
                throw new Exception("Employee Send Request Not Found");
            }

            // Assuming employeeSendRequest is an object of type Employee
            var departmentId = employeeSendRequest.DepartmentId;
            Guid? managerId = null;
            if (departmentId != null)
            {
                var manager = _dbContext.Employees
                    .Include(e => e.UserAccount)
                    .ThenInclude(ua => ua.Role)
                    .Where(e => e.DepartmentId == departmentId && e.UserAccount.Role.Name == "Manager")
                    .FirstOrDefault();

                // Now manager will be null if there is no manager in the department
                managerId = manager?.Id; // This will be null if no manager is found

                // Use managerId as needed
            }
            var requestId = Guid.NewGuid();
            // Initialize new Request object
            Request newRequest = new Request()
            {
                Id = requestId,
                EmployeeSendRequestId = employeeId,
                EmployeeSendRequest = employeeSendRequest,
                Status = RequestStatus.Pending,  // default status
                IsDeleted = false,
                RequestWorkTimeId = newRequestWorkTime.Id,
                RequestWorkTime = newRequestWorkTime,
                Message = "",
                PathAttachmentFile = dto.linkFile ?? "",
                Reason = dto.reason ?? "",
                SubmitedDate = DateTime.Now,
                requestType = RequestType.WorkTime,
                EmployeeIdLastDecider = managerId,
            };

            // Add the new Request and RequestWorkTime to the database and save changes
            await _dbContext.Requests.AddAsync(newRequest);
            await _dbContext.SaveChangesAsync();
            await SendRequestWorkTimeToManagerFirebase(requestId);

            return new
            {
                RequestWorkTimeId = newRequestWorkTime.Id,
                RequestId = newRequest.Id
            };
        }

        public async Task<object> EditRequestWorkTime(RequestWorkTimeDTO dto)
        {
            // Step 1: Retrieve the existing record from the database using its ID
            Request request = await _dbContext.Requests.Include(r => r.RequestWorkTime).FirstOrDefaultAsync(r => r.Id == dto.Id);
            RequestWorkTime existingRequestWorkTime = request.RequestWorkTime;

            // Check if the RequestWorkTime exists
            if (existingRequestWorkTime == null)
            {
                throw new Exception("RequestWorkTime not found.");
            }

            // Step 2: Update the necessary fields
            if (dto.RealHourStart != null)
            {
                existingRequestWorkTime.RealHourStart = dto.RealHourStart;
            }

            if (dto.RealHourEnd != null)
            {
                existingRequestWorkTime.RealHourEnd = dto.RealHourEnd;
            }

            if (dto.NumberOfComeLateHour != null)
            {
                existingRequestWorkTime.NumberOfComeLateHour = dto.NumberOfComeLateHour;
            }

            if (dto.NumberOfLeaveEarlyHour != null)
            {
                existingRequestWorkTime.NumberOfLeaveEarlyHour = dto.NumberOfLeaveEarlyHour;
            }

            if (dto.Name != null)
            {
                existingRequestWorkTime.Name = dto.Name;
            }
            var isRequestChange = false;

            if (dto.linkFile != null)
            {
                request.PathAttachmentFile = dto.linkFile;
                isRequestChange = true;
            }

            if (dto.reason != null)
            {
                request.Reason = dto.reason;
                isRequestChange = true;
            }

            if (dto.status != null)
            {
                request.Status = dto.status == 0 ? RequestStatus.Pending : (dto.status == 1 ? RequestStatus.Approved : RequestStatus.Rejected);
                isRequestChange = true;
            }

            if (dto.IsDeleted != null)
            {
                request.IsDeleted = (bool)dto.IsDeleted;
                existingRequestWorkTime.IsDeleted = (bool)dto.IsDeleted;
            }

            // Step 3: Save the changes to the database
            //_dbContext.RequestWorkTimes.Update(existingRequestWorkTime);
            await _dbContext.SaveChangesAsync();

            return new
            {
                RequestWorkTimeId = existingRequestWorkTime.Id,
                UpdatedFields = new
                {
                    RealHourStart = existingRequestWorkTime.RealHourStart,
                    RealHourEnd = existingRequestWorkTime.RealHourEnd,
                    NumberOfComeLateHour = existingRequestWorkTime.NumberOfComeLateHour,
                    NumberOfLeaveEarlyHour = existingRequestWorkTime.NumberOfLeaveEarlyHour
                }
            };
        }

        //public List<WorkslotEmployeeDTO> GetWorkslotEmployeesWithLessThanNineHours(Guid employeeId)
        //{
        //    var workslotEmployees = _dbContext.WorkslotEmployees
        //        .Include(w => w.Employee)
        //        .Include(w => w.Workslot)
        //        .Where(w => w.IsDeleted == false)
        //        .Where(w => w.EmployeeId == employeeId && string.IsNullOrEmpty(w.CheckInTime) == false && string.IsNullOrEmpty(w.CheckOutTime) == false)
        //        .ToList();

        //    var result = new List<WorkslotEmployeeDTO>();

        //    foreach (var workslotEmployee in workslotEmployees)
        //    {
        //        double duration = DateTime.ParseExact(workslotEmployee.CheckOutTime, "HH:mm", CultureInfo.InvariantCulture).Hour - DateTime.ParseExact(workslotEmployee.CheckInTime, "HH:mm", CultureInfo.InvariantCulture).Hour;

        //        if (duration < 4)
        //        {
        //            var request = _dbContext.RequestWorkTimes.FirstOrDefault(rw => rw.WorkslotEmployeeId == workslotEmployee.Id);
        //            if (request != null)
        //            {
        //                var request = _dbContext.Requests.FirstOrDefault(r => r.RequestWorkTimeId == request.Id);
        //                result.Add(new WorkslotEmployeeDTO
        //                {
        //                    workslotEmployeeId = workslotEmployee.Id,
        //                    Date = workslotEmployee.Workslot.DateOfSlot,
        //                    SlotStart = workslotEmployee.Workslot.FromHour,
        //                    RequestId = request.Id,
        //                    SlotEnd = workslotEmployee.Workslot.ToHour,
        //                    CheckInTime = workslotEmployee.CheckInTime,
        //                    CheckOutTime = workslotEmployee.CheckOutTime,
        //                    TimeLeaveEarly = (DateTime.ParseExact(workslotEmployee.Workslot.ToHour, "HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(workslotEmployee.CheckOutTime, "HH:mm", CultureInfo.InvariantCulture)).TotalHours,
        //                    TimeComeLate = (DateTime.ParseExact(workslotEmployee.CheckInTime, "HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(workslotEmployee.Workslot.FromHour, "HH:mm", CultureInfo.InvariantCulture)).TotalHours,
        //                    statusName = request.Status.ToString(),
        //                    reason = request.Reason,
        //                    linkFile = request.PathAttachmentFile
        //                });
        //            } else
        //            {
        //                result.Add(new WorkslotEmployeeDTO
        //                {
        //                    workslotEmployeeId = workslotEmployee.Id,
        //                    Date = workslotEmployee.Workslot.DateOfSlot,
        //                    SlotStart = workslotEmployee.Workslot.FromHour,
        //                    SlotEnd = workslotEmployee.Workslot.ToHour,
        //                    CheckInTime = workslotEmployee.CheckInTime,
        //                    CheckOutTime = workslotEmployee.CheckOutTime,
        //                    TimeLeaveEarly = (DateTime.ParseExact(workslotEmployee.Workslot.ToHour, "HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(workslotEmployee.CheckOutTime, "HH:mm", CultureInfo.InvariantCulture)).TotalHours,
        //                    TimeComeLate = (DateTime.ParseExact(workslotEmployee.CheckInTime, "HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(workslotEmployee.Workslot.FromHour, "HH:mm", CultureInfo.InvariantCulture)).TotalHours,
        //                    statusName = "Lack Of Work Time",
        //                    reason = null,
        //                    linkFile = null
        //                });
        //            }
        //        }
        //    }

        //    return result;
        //}

        //public List<WorkslotEmployeeDTO> GetWorkslotEmployeesWithLessThanNineHours(Guid employeeId)
        //{
        //    var workslotEmployees = _dbContext.WorkslotEmployees
        //        .Include(w => w.Employee)
        //        .Include(w => w.Workslot)
        //        .Where(w => w.IsDeleted == false && w.EmployeeId == employeeId)
        //        .ToList();

        //    var result = new List<WorkslotEmployeeDTO>();

        //    var groupedByDate = workslotEmployees.GroupBy(w => w.Workslot.DateOfSlot);

        //    foreach (var group in groupedByDate)
        //    {
        //        var morningSlot = group.FirstOrDefault(w => w.Workslot.IsMorning);
        //        var morningSlot = group.FirstOrDefault(w => !w.Workslot.IsMorning);

        //        if (morningSlot != null && morningSlot != null &&
        //            !string.IsNullOrEmpty(morningSlot.CheckInTime) &&
        //            !string.IsNullOrEmpty(morningSlot.CheckOutTime))
        //        {
        //            double duration = DateTime.ParseExact(morningSlot.CheckOutTime, "HH:mm", CultureInfo.InvariantCulture).Subtract(DateTime.ParseExact(morningSlot.CheckInTime, "HH:mm", CultureInfo.InvariantCulture)).TotalHours;

        //            if (duration < 9)
        //            {
        //                var request = _dbContext.RequestWorkTimes.FirstOrDefault(rw => rw.WorkslotEmployeeId == morningSlot.Id);
        //                if (request != null)
        //                {
        //                    var request = _dbContext.Requests.FirstOrDefault(r => r.RequestWorkTimeId == request.Id);
        //                    result.Add(new WorkslotEmployeeDTO
        //                    {
        //                        workslotEmployeeId = morningSlot.Id,
        //                        Date = morningSlot.Workslot.DateOfSlot,
        //                        SlotStart = morningSlot.Workslot.FromHour,
        //                        RequestId = request.Id,
        //                        SlotEnd = morningSlot.Workslot.ToHour,
        //                        CheckInTime = morningSlot.CheckInTime,
        //                        CheckOutTime = morningSlot.CheckOutTime,
        //                        TimeLeaveEarly = (DateTime.ParseExact(morningSlot.Workslot.ToHour, "HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(morningSlot.CheckOutTime, "HH:mm", CultureInfo.InvariantCulture)).TotalHours,
        //                        TimeComeLate = (DateTime.ParseExact(morningSlot.CheckInTime, "HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(morningSlot.Workslot.FromHour, "HH:mm", CultureInfo.InvariantCulture)).TotalHours,
        //                        statusName = request.Status.ToString(),
        //                        reason = request.Reason,
        //                        linkFile = request.PathAttachmentFile
        //                    });
        //                }
        //                else
        //                {
        //                    result.Add(new WorkslotEmployeeDTO
        //                    {
        //                        workslotEmployeeId = morningSlot.Id,
        //                        Date = morningSlot.Workslot.DateOfSlot,
        //                        SlotStart = morningSlot.Workslot.FromHour,
        //                        SlotEnd = morningSlot.Workslot.ToHour,
        //                        CheckInTime = morningSlot.CheckInTime,
        //                        CheckOutTime = morningSlot.CheckOutTime,
        //                        TimeLeaveEarly = (DateTime.ParseExact(morningSlot.Workslot.ToHour, "HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(morningSlot.CheckOutTime, "HH:mm", CultureInfo.InvariantCulture)).TotalHours,
        //                        TimeComeLate = (DateTime.ParseExact(morningSlot.CheckInTime, "HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(morningSlot.Workslot.FromHour, "HH:mm", CultureInfo.InvariantCulture)).TotalHours,
        //                        statusName = "Lack Of Work Time",
        //                        reason = null,
        //                        linkFile = null
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}

        public List<WorkslotEmployeeDTO> GetWorkslotEmployeesWithLessThanNineHours(Guid employeeId, bool? isWorkLate = false, bool? isLeaveSoon = false, bool? isNotCheckIn = false, bool? isNotCheckOut = false)
        {
            var workslotEmployees = _dbContext.WorkslotEmployees
                .Include(w => w.Employee).ThenInclude(e => e.Department)
                .Include(w => w.Workslot)
                .Include(w => w.AttendanceStatus)
                .ThenInclude(a => a.WorkingStatus)
                .Where(w => w.IsDeleted == false && w.EmployeeId == employeeId)
                .ToList();

            var result = new List<WorkslotEmployeeDTO>();

            var groupedByDate = workslotEmployees.GroupBy(w => w.Workslot.DateOfSlot);

            foreach (var group in groupedByDate)
            {
                var morningSlot = group.FirstOrDefault(w => w.Workslot.IsMorning);
                var afternoonSlot = group.FirstOrDefault(w => !w.Workslot.IsMorning);

                if (morningSlot != null && afternoonSlot != null)
                {
                    var rejectAndCancelRequestWorkTime = _dbContext.Requests
                        .Where(r => r.RequestWorkTime != null)
                        .Where(r => r.Status == RequestStatus.Rejected || r.Status == RequestStatus.Cancel)
                        .Select(r => r.RequestWorkTimeId);
                    var requestWorkTime = _dbContext.RequestWorkTimes
                        .Where(r => !rejectAndCancelRequestWorkTime.Contains(r.Id))
                        .FirstOrDefault(rw => rw.WorkslotEmployeeId == afternoonSlot.Id);
                    if (requestWorkTime != null)
                    {
                        continue;
                        //var request = _dbContext.Requests.FirstOrDefault(r => r.RequestWorkTimeId == requestWorkTime.Id);
                        //result.Add(new WorkslotEmployeeDTO
                        //{
                        //    workslotEmployeeId = morningSlot.Id,
                        //    Date = morningSlot.Workslot.DateOfSlot,
                        //    SlotStart = morningSlot.Workslot.FromHour,
                        //    SlotEnd = morningSlot.Workslot.ToHour,
                        //    CheckInTime = morningSlot.CheckInTime,
                        //    CheckOutTime = morningSlot.CheckOutTime,
                        //    TimeLeaveEarly = CalculateTimeDifference(morningSlot.Workslot.ToHour, morningSlot.CheckOutTime, true),
                        //    TimeComeLate = CalculateTimeDifference(morningSlot.CheckInTime, morningSlot.Workslot.FromHour, false),
                        //    deciderId = request.EmployeeIdLastDecider,
                        //    deciderName = request.EmployeeIdLastDecider != null ? _dbContext.Employees.FirstOrDefault(e => e.Id == request.EmployeeIdLastDecider)?.FirstName : null,
                        //    statusName = request.Status.ToString(),
                        //    reason = request.Reason,
                        //    linkFile = request.PathAttachmentFile,
                        //    RequestId = request.Id,
                        //});
                    }
                    else
                    {
                        if (morningSlot.CheckInTime == null) continue;
                            //if (morningSlot.Employee.DepartmentId == null) continue;
                            var checkIn = DateTime.ParseExact(morningSlot.CheckInTime, "HH:mm", CultureInfo.InvariantCulture);
                        if (afternoonSlot.CheckOutTime == null)
                        {
                            result.Add(new WorkslotEmployeeDTO
                            {
                                workslotEmployeeId = afternoonSlot.Id,
                                workslotEmployeeMorningId = morningSlot.Id,
                                attendanceStatusMorningId = morningSlot.AttendanceStatusId,
                                attendanceStatusAfternoonId = afternoonSlot.AttendanceStatusId,
                                Date = afternoonSlot.Workslot.DateOfSlot,
                                SlotStart = morningSlot.Workslot.FromHour,
                                SlotEnd = afternoonSlot.Workslot.ToHour,
                                CheckInTime = morningSlot.CheckInTime,
                                CheckOutTime = afternoonSlot.CheckOutTime,
                                TimeLeaveEarly = CalculateTimeDifference(afternoonSlot.Workslot.ToHour, afternoonSlot.CheckOutTime, true),
                                TimeComeLate = "00:00",
                                deciderId = _dbContext.Departments.FirstOrDefault(d => d.Id == afternoonSlot.Employee.DepartmentId)?.ManagerId,
                                deciderName = null,
                                statusName = "Not Check Out Yet",
                                reason = null,
                                linkFile = null
                            });
                            continue;
                        }
                            var checkOut = DateTime.ParseExact(afternoonSlot.CheckOutTime, "HH:mm", CultureInfo.InvariantCulture);
                            var totalWorkDuration = (checkOut - checkIn).TotalHours;

                            // Calculate TimeLeaveEarly and TimeComeLate
                            var timeLeaveEarly = CalculateTimeDifference(afternoonSlot.Workslot.ToHour, afternoonSlot.CheckOutTime, false);
                            var timeComeLate = CalculateTimeDifference(morningSlot.Workslot.FromHour, morningSlot.CheckInTime, true);

                            // Check the total work duration and ensure that time differences are not "00:00"
                            if (totalWorkDuration < 9 && (timeLeaveEarly != "00:00" || timeComeLate != "00:00"))
                            {
                                var deciderId = _dbContext.Departments.FirstOrDefault(d => d.Id == afternoonSlot.Employee.DepartmentId)?.ManagerId;
                                var decider = deciderId != null ? _dbContext.Employees.FirstOrDefault(e => e.Id == deciderId) : null;
                                var deciderName = decider != null ? decider.FirstName + " " + decider.LastName : null;

                                result.Add(new WorkslotEmployeeDTO
                                {
                                    workslotEmployeeId = afternoonSlot.Id,
                                    workslotEmployeeMorningId = morningSlot.Id,
                                    attendanceStatusMorningId = morningSlot.AttendanceStatusId,
                                    attendanceStatusAfternoonId = afternoonSlot.AttendanceStatusId,
                                    Date = afternoonSlot.Workslot.DateOfSlot,
                                    SlotStart = morningSlot.Workslot.FromHour,
                                    SlotEnd = afternoonSlot.Workslot.ToHour,
                                    CheckInTime = morningSlot.CheckInTime,
                                    CheckOutTime = afternoonSlot.CheckOutTime,
                                    TimeLeaveEarly = timeLeaveEarly,
                                    TimeComeLate = timeComeLate,
                                    deciderId = deciderId,
                                    deciderName = deciderName,
                                    statusName = "Lack Of Work Time",
                                    reason = null,
                                    linkFile = null
                                });
                            }
                        

                    }
                }
            }

            // Apply all filters before converting to list
            IEnumerable<WorkslotEmployeeDTO> filteredResults = result.AsEnumerable();
            if (isWorkLate.GetValueOrDefault())
            {
                filteredResults = filteredResults.Where(r => r.TimeComeLate != "00:00");
            }
            if (isLeaveSoon.GetValueOrDefault())
            {
                filteredResults = filteredResults.Where(r => r.TimeLeaveEarly != "00:00");
            }
            if (isNotCheckIn.GetValueOrDefault())
            {
                filteredResults = filteredResults.Where(r => string.IsNullOrEmpty(r.CheckInTime));
            }
            if (isNotCheckOut.GetValueOrDefault())
            {
                filteredResults = filteredResults.Where(r => string.IsNullOrEmpty(r.CheckOutTime));
            }

            return filteredResults.ToList();
        }

        private string CalculateTimeDifference(string expectedTime, string actualTime, bool isLate)
        {
            if (string.IsNullOrEmpty(actualTime) || string.IsNullOrEmpty(expectedTime))
                return "00:00";

            var expected = DateTime.ParseExact(expectedTime, "HH:mm", CultureInfo.InvariantCulture);
            var actual = DateTime.ParseExact(actualTime, "HH:mm", CultureInfo.InvariantCulture);
            var timeSpan = isLate ? actual - expected : expected - actual;

            return timeSpan.TotalMinutes > 0 ? timeSpan.ToString(@"hh\:mm") : "00:00";
        }

        public List<RequestWorkTimeDTO> GetAllRequestWorkTime(string? nameSearch, int? status, string? month, Guid? employeeIdd = null)
        {
            var result = new List<RequestWorkTimeDTO>();
            var list = _dbContext.Requests
                        .Include(r => r.RequestWorkTime)
                        .ThenInclude(rw => rw.WorkslotEmployee)
                        .ThenInclude(we => we.Workslot)
                        .Where(r => r.IsDeleted == false && r.requestType == RequestType.WorkTime);

            if (status.HasValue && status != -1)
            {
                list = list.Where(r => (int)r.Status == status.Value);
            }

            // Filter by manager's department if employeeId is provided
            Guid? departmentId = null;
            if (employeeIdd.HasValue)
            {
                var manager = _dbContext.Employees
                    .Include(e => e.UserAccount)
                    .ThenInclude(ua => ua.Role)
                    .FirstOrDefault(e => e.Id == employeeIdd);

                if (manager != null && manager.UserAccount.Role.Name == "Manager")
                {
                    departmentId = manager.DepartmentId;
                }
                else
                {
                    throw new Exception("EmployeeId is not Manager");
                }
            }

            if (departmentId.HasValue)
            {
                list = list.Where(r => r.EmployeeSendRequest.DepartmentId == departmentId);
            }

            // Processing the filter only if month is provided
            int? monthFilter = null;
            int? yearFilter = null;
            if (!string.IsNullOrEmpty(month))
            {
                var dateFilter = DateTime.ParseExact(month, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                monthFilter = dateFilter.Month;
                yearFilter = dateFilter.Year;
            }

            foreach (var r in list.ToList()) // Materialize the query to avoid repeated database calls
            {
                var employee = _dbContext.Employees.FirstOrDefault(e => e.IsDeleted == false && e.Id == r.EmployeeSendRequestId);
                if (employee == null) continue;

                var allHourWT = _dbContext.Requests
                    .Where(r => r.EmployeeSendRequestId == employee.Id && r.Status == RequestStatus.Approved && r.requestType == RequestType.WorkTime)
                    .Select(r => r.RequestWorkTime);

                var employeeId = employee.Id;
                var employeeName = employee.FirstName + " " + employee.LastName;

                var timeInMonth = 0;
                var timeInYear = 0;

                if (monthFilter.HasValue && yearFilter.HasValue)
                {
                    timeInMonth = allHourWT.Count(wt => wt.DateOfSlot.HasValue && wt.DateOfSlot.Value.Month == monthFilter.Value && wt.DateOfSlot.Value.Year == yearFilter.Value);
                    timeInYear = allHourWT.Count(wt => wt.DateOfSlot.HasValue && wt.DateOfSlot.Value.Year == yearFilter.Value);
                }
                else
                {
                    timeInYear = allHourWT.Count(wt => wt.DateOfSlot.HasValue);
                }

                result.Add(new RequestWorkTimeDTO
                {
                    Id = r.Id,
                    employeeId = employee?.Id ?? Guid.Empty,
                    employeeName = employee?.FirstName + " " + employee?.LastName,
                    employeeNumber = employee?.EmployeeNumber,
                    deciderId = r.EmployeeIdLastDecider,
                    RealHourStart = r.RequestWorkTime?.RealHourStart,
                    RealHourEnd = r.RequestWorkTime?.RealHourEnd,
                    NumberOfComeLateHour = r.RequestWorkTime?.NumberOfComeLateHour ?? null,
                    NumberOfLeaveEarlyHour = r.RequestWorkTime?.NumberOfLeaveEarlyHour ?? null,
                    TimeInMonth = timeInMonth,
                    TimeInYear = timeInYear,
                    WorkslotEmployeeId = r.RequestWorkTime?.WorkslotEmployeeId ?? Guid.Empty,
                    SlotStart = r.RequestWorkTime?.WorkslotEmployee?.Workslot?.FromHour,
                    SlotEnd = r.RequestWorkTime?.WorkslotEmployee?.Workslot?.ToHour,
                    DateOfWorkTime = r.RequestWorkTime?.DateOfSlot?.ToString("yyyy/MM/dd"),
                    linkFile = r.PathAttachmentFile,
                    Name = r.RequestWorkTime?.Name,
                    submitDate = r.SubmitedDate.ToString("yyyy/MM/dd"),
                    status = (int)r.Status,
                    statusName = r.Status.ToString(),
                    reason = r.Reason,
                    reasonReject = r.Message,
                    IsDeleted = r.RequestWorkTime?.IsDeleted ?? false
                });
            }

            if (!string.IsNullOrEmpty(nameSearch))
            {
                result = result.Where(r => r.employeeName.ToLower().Contains(nameSearch.ToLower())).ToList();
            }

            return result;
        }

        public async Task<object> ApproveRequestWorkTime(Guid requestId)
        {
            // Step 1: Retrieve the Request by requestId
            var request = await _dbContext.Requests
                                          .Include(r => r.RequestWorkTime)
                                          .ThenInclude(rwt => rwt.WorkslotEmployee)
                                          .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                return new { message = "Request not found" };
            }

            // Update the Request status to Approve
            request.Status = RequestStatus.Approved;

            // Step 2: Find all WorkslotEmployees that should be updated
            var dateOfTime = request.RequestWorkTime.DateOfSlot;

            //var workslotEmployees = await _dbContext.WorkslotEmployees
            //                                        .Include(we => we.Workslot)
            //                                        .Where(we => we.EmployeeId == request.EmployeeSendRequestId)
            //                                        .Where(we => we.Workslot.DateOfSlot == dateOfTime)
            //                                        .ToListAsync();

            var morningId = request.RequestWorkTime.WorkslotEmployeeMorningId;
            var morningSlot = _dbContext.WorkslotEmployees.FirstOrDefault(we => we.Id == morningId);
            var afternoonSlot = request.RequestWorkTime.WorkslotEmployee;

            // Step 3: Update the AttendanceStatus for these WorkslotEmployees
            var newAttendanceStatus = await _dbContext.AttendanceStatuses
                                                      .Include(att => att.WorkingStatus)
                                                      .FirstOrDefaultAsync(att => att.WorkingStatus != null && att.WorkingStatus.Name == "Worked");

            if (newAttendanceStatus == null)
            {
                throw new Exception("Attendance status for the WorkingStatus 'Worked' not found");
            }

            if (morningSlot != null)
            {
                morningSlot.AttendanceStatus = newAttendanceStatus;
                morningSlot.AttendanceStatusId = newAttendanceStatus.Id;
                morningSlot.CheckInTime = morningSlot.Workslot.FromHour;
                morningSlot.CheckOutTime = morningSlot.Workslot.ToHour;
            }

            if (afternoonSlot != null)
            {
                afternoonSlot.AttendanceStatus = newAttendanceStatus;
                afternoonSlot.AttendanceStatusId = newAttendanceStatus.Id;
                afternoonSlot.CheckInTime = afternoonSlot.Workslot.FromHour;
                afternoonSlot.CheckOutTime = afternoonSlot.Workslot.ToHour;
            }


            // Step 4: Save changes to the database
            await _dbContext.SaveChangesAsync();
            await SendRequestWorkTimeToEmployeeFirebase(requestId);

            return new { message = "RequestWorkTime approved and WorkslotEmployee updated successfully\n morningSlot: "+ morningId + "\nafternoon: " + afternoonSlot?.Id };
        }

        public async Task<object> RejectWorkTimeRequest(RequestReasonDTO requestObj)
        {
            Guid requestId = requestObj.requestId;
            string reason = requestObj.reason;

            // Retrieve the work time request by requestId
            var request = await _dbContext.Requests
                                           .Include(r => r.RequestWorkTime)
                                           .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                throw new Exception("Request not found.");
            }

            if (request.RequestWorkTime == null)
            {
                throw new Exception("Work Time Request not found.");
            }

            // Check if the request is indeed approved; only approved requests can be rejected
            if (request.Status == RequestStatus.Approved)
            {
                throw new Exception("Only Non-approved requests can be rejected.");
            }

            // Set the Request status to Rejected based on your application logic
            request.Status = RequestStatus.Rejected;
            request.Message = reason;
            request.EmployeeIdLastDecider = requestObj.employeeIdDecider;

            // Save the changes to the database
            await _dbContext.SaveChangesAsync();
            await SendRequestWorkTimeToEmployeeFirebase(requestId);

            return new { message = "Work Time request rejected successfully." };
        }

        public async Task<object> CancelApprovedWorkTimeRequest(RequestReasonDTO requestObj)
        {
            Guid requestId = requestObj.requestId;
            string reason = requestObj.reason;

            // Retrieve the request by requestId
            var request = await _dbContext.Requests
                                           .Include(r => r.RequestWorkTime)
                                           .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                throw new Exception("Request not found.");
            }

            if (request.RequestWorkTime == null)
            {
                throw new Exception("Work Time Request not found.");
            }

            // Check if the request is indeed approved; only approved requests can be cancelled
            if (request.Status != RequestStatus.Approved)
            {
                throw new Exception("Only approved requests can be cancelled.");
            }

            // Set the Request status to Cancelled based on your application logic
            request.Status = RequestStatus.Cancel; // Assuming Cancelled is a defined status in your system
            request.Message = reason;
            request.EmployeeIdLastDecider = requestObj.employeeIdDecider;

            // Find all WorkslotEmployees that were updated due to this request and reset their attendance status
            var dateOfRequest = request.RequestWorkTime.DateOfSlot;

            var workslotEmployees = await _dbContext.WorkslotEmployees
                                                    .Include(we => we.Workslot)
                                                    .Where(we => we.EmployeeId == request.EmployeeSendRequestId && we.Workslot.DateOfSlot == dateOfRequest)
                                                    .ToListAsync();

            var morningId = request.RequestWorkTime.WorkslotEmployeeMorningId;
            var morningSlot = _dbContext.WorkslotEmployees.FirstOrDefault(we => we.Id == morningId);
            var afternoonSlot = request.RequestWorkTime.WorkslotEmployee;

            // Assuming you have a default or previous attendance status to reset to; adjust this logic as necessary
            var defaultAttendanceStatusMorning = await _dbContext.AttendanceStatuses.FirstOrDefaultAsync(att => att.Id == request.RequestWorkTime.attendanceStatusMorningId);
            var defaultAttendanceStatusAfternoon = await _dbContext.AttendanceStatuses.FirstOrDefaultAsync(att => att.Id == request.RequestWorkTime.attendanceStatusAfternoonId);

            if (morningSlot != null)
            {
                morningSlot.AttendanceStatus = defaultAttendanceStatusMorning;
                morningSlot.AttendanceStatusId = defaultAttendanceStatusMorning.Id;
                morningSlot.CheckInTime = request.RequestWorkTime.RealHourStart;
                morningSlot.CheckOutTime = morningSlot.Workslot.ToHour;
            }

            if (afternoonSlot != null)
            {
                afternoonSlot.AttendanceStatus = defaultAttendanceStatusAfternoon;
                afternoonSlot.AttendanceStatusId = defaultAttendanceStatusAfternoon.Id;
                afternoonSlot.CheckInTime = afternoonSlot.Workslot.FromHour;
                afternoonSlot.CheckOutTime = request.RequestWorkTime.RealHourEnd;
            }

            // Save the changes to the database
            await _dbContext.SaveChangesAsync();
            await SendRequestWorkTimeToEmployeeFirebase(requestId);

            return new { message = "Work Time request cancelled successfully." };
        }

        public async Task<bool> SendRequestWorkTimeToManagerFirebase(Guid requestId)
        {
            string managerPath = "/managerNoti";
            return await SendWorkTimeRequestStatusToFirebase(requestId, managerPath);
        }

        public async Task<bool> SendRequestWorkTimeToEmployeeFirebase(Guid requestId)
        {
            string employeePath = "/employeeNoti";
            return await SendWorkTimeRequestStatusToFirebase(requestId, employeePath);
        }

        public async Task<bool> SendWorkTimeRequestStatusToFirebase(Guid requestId, string path)
        {
            var request = await _dbContext.Requests
                .Include(r => r.RequestWorkTime)
                .Include(r => r.EmployeeSendRequest)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                return false; // Request not found
            }

            var manager = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeIdLastDecider);

            // Generate a reverse timestamp key for sorting in Firebase (newer entries first)
            long reverseTimestamp = DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks;

            var firebaseData = new
            {
                requestId = request.Id,
                employeeSenderId = request.EmployeeSendRequestId,
                employeeSenderName = request.EmployeeSendRequest?.FirstName + " " + request.EmployeeSendRequest?.LastName,
                employeeSenderNumber = request.EmployeeSendRequest?.EmployeeNumber,
                employeeDeciderId = request.EmployeeIdLastDecider,
                employeeDeciderName = manager?.FirstName + " " + manager?.LastName,
                workTimeId = request.RequestWorkTimeId,
                status = request.Status.ToString(),
                reason = request.Reason,
                messageOfDecider = request.Message,
                submitedDate = request.SubmitedDate,
                dateOfWorkTime = request.RequestWorkTime?.DateOfSlot,
                realHourStart = request.RequestWorkTime?.RealHourStart,
                realHourEnd = request.RequestWorkTime?.RealHourEnd,
                actionDate = DateTime.Now,
                requestType = "Work Time",
                isSeen = false
            };

            var json = JsonSerializer.Serialize(firebaseData);
            var httpClient = new HttpClient();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Append the reverse timestamp key to the path to ensure ordering
            var result = await httpClient.PutAsync($"https://nextjs-course-f2de1-default-rtdb.firebaseio.com{path}/{reverseTimestamp}.json", content);

            return result.IsSuccessStatusCode;
        }

        public async Task<bool> SoftDeleteRequestWorkTime(Guid requestId)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    // Fetch the request work time and the related request
                    var request = await _dbContext.Requests.Include(rwt => rwt.RequestWorkTime).FirstOrDefaultAsync(rwt => rwt.Id == requestId);
                    if (request == null)
                    {
                        throw new Exception("RequestWorkTime not found.");
                    }

                    // Mark the RequestWorkTime as deleted
                    request.IsDeleted = true;

                    // If there's a linked Request, mark it as deleted too
                    if (request.RequestWorkTime != null)
                    {
                        request.RequestWorkTime.IsDeleted = true;

                        // Fetch related WorkslotEmployees if any and mark them as deleted
                        var workslotEmployees = await _dbContext.WorkslotEmployees
                                                                .Where(we => we.Id == request.RequestWorkTime.WorkslotEmployeeId)
                                                                .ToListAsync();

                        workslotEmployees.ForEach(we => we.IsDeleted = true);
                    }

                    // Save changes to the database
                    await _dbContext.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Failed to delete RequestWorkTime: " + ex.Message);
                }
            }
        }

        public async Task<RequestWorkTimeDTO> GetRequestWorkTimeById(Guid requestId)
        {
            var request = await _dbContext.Requests
                .Include(r => r.RequestWorkTime)
                    .ThenInclude(rwt => rwt.WorkslotEmployee)
                        .ThenInclude(we => we.Workslot)
                .Include(r => r.EmployeeSendRequest)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.requestType == RequestType.WorkTime && !r.IsDeleted);

            if (request == null)
            {
                throw new Exception("Request not found or has been deleted.");
            }

            var requestWorkTime = request.RequestWorkTime;
            var employee = request.EmployeeSendRequest;

            var dto = new RequestWorkTimeDTO
            {
                Id = request.Id,
                employeeId = employee.Id,
                employeeName = $"{employee.FirstName} {employee.LastName}",
                employeeNumber = employee.EmployeeNumber,
                deciderId = request.EmployeeIdLastDecider,
                RealHourStart = requestWorkTime?.RealHourStart,
                RealHourEnd = requestWorkTime?.RealHourEnd,
                NumberOfComeLateHour = requestWorkTime?.NumberOfComeLateHour ?? null,
                NumberOfLeaveEarlyHour = requestWorkTime?.NumberOfLeaveEarlyHour ?? null,
                WorkslotEmployeeId = requestWorkTime?.WorkslotEmployeeId ?? Guid.Empty,
                SlotStart = requestWorkTime?.WorkslotEmployee?.Workslot?.FromHour,
                SlotEnd = requestWorkTime?.WorkslotEmployee?.Workslot?.ToHour,
                DateOfWorkTime = requestWorkTime?.DateOfSlot?.ToString("yyyy/MM/dd"),
                linkFile = request.PathAttachmentFile,
                Name = requestWorkTime?.Name,
                submitDate = request.SubmitedDate.ToString("yyyy/MM/dd"),
                status = (int)request.Status,
                statusName = request.Status.ToString(),
                reason = request.Reason,
                reasonReject = request.Message,
                IsDeleted = requestWorkTime?.IsDeleted ?? false
            };

            return dto;
        }


    }
}
