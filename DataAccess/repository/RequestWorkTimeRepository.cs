using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Globalization;
using System.Runtime.CompilerServices;

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
                result.Add(new RequestWorkTimeDTO()
                {
                    Id = r.Id,
                    Name = r.RequestWorkTime.Name,
                    employeeName = employee.FirstName + " " + employee.LastName,
                    RealHourStart = r.RequestWorkTime.RealHourStart,
                    RealHourEnd = r.RequestWorkTime.RealHourEnd,
                    NumberOfComeLateHour = r.RequestWorkTime.NumberOfComeLateHour,
                    NumberOfLeaveEarlyHour = r.RequestWorkTime.NumberOfComeLateHour,
                    DateOfWorkTime = r.RequestWorkTime.DateOfSlot?.ToString("yyyy/MM/dd"),
                    status = (int)r.Status,
                    statusName = r.Status.ToString(),
                    reason = r.Reason,
                    linkFile = r.PathAttachmentFile,
                    WorkslotEmployeeId = r.RequestWorkTime.WorkslotEmployeeId
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
                IsDeleted = false  // Set the soft delete flag to false
            };

            await _dbContext.RequestWorkTimes.AddAsync(newRequestWorkTime);

            // Initialize new Request object
            Request newRequest = new Request()
            {
                Id = Guid.NewGuid(),
                EmployeeSendRequestId = employeeId,
                Status = RequestStatus.Pending,  // default status
                IsDeleted = false,
                RequestWorkTimeId = newRequestWorkTime.Id,
                RequestWorkTime = newRequestWorkTime,
                Message = "",
                PathAttachmentFile = dto.linkFile ?? "",
                Reason = dto.reason ?? "",
                SubmitedDate = DateTime.Now,
                requestType = RequestType.WorkTime
            };

            // Add the new Request and RequestWorkTime to the database and save changes
            await _dbContext.Requests.AddAsync(newRequest);
            await _dbContext.SaveChangesAsync();

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
        //            var requestWorkTime = _dbContext.RequestWorkTimes.FirstOrDefault(rw => rw.WorkslotEmployeeId == workslotEmployee.Id);
        //            if (requestWorkTime != null)
        //            {
        //                var request = _dbContext.Requests.FirstOrDefault(r => r.RequestWorkTimeId == requestWorkTime.Id);
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
        //        var afternoonSlot = group.FirstOrDefault(w => !w.Workslot.IsMorning);

        //        if (morningSlot != null && afternoonSlot != null &&
        //            !string.IsNullOrEmpty(morningSlot.CheckInTime) &&
        //            !string.IsNullOrEmpty(afternoonSlot.CheckOutTime))
        //        {
        //            double duration = DateTime.ParseExact(afternoonSlot.CheckOutTime, "HH:mm", CultureInfo.InvariantCulture).Subtract(DateTime.ParseExact(morningSlot.CheckInTime, "HH:mm", CultureInfo.InvariantCulture)).TotalHours;

        //            if (duration < 9)
        //            {
        //                var requestWorkTime = _dbContext.RequestWorkTimes.FirstOrDefault(rw => rw.WorkslotEmployeeId == afternoonSlot.Id);
        //                if (requestWorkTime != null)
        //                {
        //                    var request = _dbContext.Requests.FirstOrDefault(r => r.RequestWorkTimeId == requestWorkTime.Id);
        //                    result.Add(new WorkslotEmployeeDTO
        //                    {
        //                        workslotEmployeeId = afternoonSlot.Id,
        //                        Date = afternoonSlot.Workslot.DateOfSlot,
        //                        SlotStart = morningSlot.Workslot.FromHour,
        //                        RequestId = request.Id,
        //                        SlotEnd = afternoonSlot.Workslot.ToHour,
        //                        CheckInTime = morningSlot.CheckInTime,
        //                        CheckOutTime = afternoonSlot.CheckOutTime,
        //                        TimeLeaveEarly = (DateTime.ParseExact(afternoonSlot.Workslot.ToHour, "HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(afternoonSlot.CheckOutTime, "HH:mm", CultureInfo.InvariantCulture)).TotalHours,
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
        //                        workslotEmployeeId = afternoonSlot.Id,
        //                        Date = afternoonSlot.Workslot.DateOfSlot,
        //                        SlotStart = morningSlot.Workslot.FromHour,
        //                        SlotEnd = afternoonSlot.Workslot.ToHour,
        //                        CheckInTime = morningSlot.CheckInTime,
        //                        CheckOutTime = afternoonSlot.CheckOutTime,
        //                        TimeLeaveEarly = (DateTime.ParseExact(afternoonSlot.Workslot.ToHour, "HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(afternoonSlot.CheckOutTime, "HH:mm", CultureInfo.InvariantCulture)).TotalHours,
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

        public List<WorkslotEmployeeDTO> GetWorkslotEmployeesWithLessThanNineHours(Guid employeeId)
        {
            var workslotEmployees = _dbContext.WorkslotEmployees
                .Include(w => w.Employee)
                .Include(w => w.Workslot)
                .Include(w => w.AttendanceStatus)
                .ThenInclude(a => a.WorkingStatus)
                .Where(w => w.IsDeleted == false && w.EmployeeId == employeeId && w.AttendanceStatus.WorkingStatus.Name == "Lack of Time")
                .ToList();

            var result = new List<WorkslotEmployeeDTO>();

            var groupedByDate = workslotEmployees.GroupBy(w => w.Workslot.DateOfSlot);

            foreach (var group in groupedByDate)
            {
                var morningSlot = group.FirstOrDefault(w => w.Workslot.IsMorning);
                var afternoonSlot = group.FirstOrDefault(w => !w.Workslot.IsMorning);

                if (morningSlot != null && afternoonSlot != null)
                {
                    var requestWorkTime = _dbContext.RequestWorkTimes.FirstOrDefault(rw => rw.WorkslotEmployeeId == afternoonSlot.Id);
                    if (requestWorkTime != null)
                    {
                        var request = _dbContext.Requests.FirstOrDefault(r => r.RequestWorkTimeId == requestWorkTime.Id);
                        result.Add(new WorkslotEmployeeDTO
                        {
                            workslotEmployeeId = afternoonSlot.Id,
                            Date = afternoonSlot.Workslot.DateOfSlot,
                            SlotStart = morningSlot.Workslot.FromHour,
                            RequestId = request.Id,
                            SlotEnd = afternoonSlot.Workslot.ToHour,
                            CheckInTime = morningSlot.CheckInTime,
                            CheckOutTime = afternoonSlot.CheckOutTime,
                            TimeLeaveEarly = (DateTime.ParseExact(afternoonSlot.Workslot.ToHour, "HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(afternoonSlot.CheckOutTime, "HH:mm", CultureInfo.InvariantCulture)).TotalHours,
                            TimeComeLate = (DateTime.ParseExact(morningSlot.CheckInTime, "HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(morningSlot.Workslot.FromHour, "HH:mm", CultureInfo.InvariantCulture)).TotalHours,
                            statusName = request.Status.ToString(),
                            reason = request.Reason,
                            linkFile = request.PathAttachmentFile
                        });
                    }
                    else
                    {
                        result.Add(new WorkslotEmployeeDTO
                        {
                            workslotEmployeeId = afternoonSlot.Id,
                            Date = afternoonSlot.Workslot.DateOfSlot,
                            SlotStart = morningSlot.Workslot.FromHour,
                            SlotEnd = afternoonSlot.Workslot.ToHour,
                            CheckInTime = morningSlot.CheckInTime,
                            CheckOutTime = afternoonSlot.CheckOutTime,
                            TimeLeaveEarly = (DateTime.ParseExact(afternoonSlot.Workslot.ToHour, "HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(afternoonSlot.CheckOutTime, "HH:mm", CultureInfo.InvariantCulture)).TotalHours,
                            TimeComeLate = (DateTime.ParseExact(morningSlot.CheckInTime, "HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(morningSlot.Workslot.FromHour, "HH:mm", CultureInfo.InvariantCulture)).TotalHours,
                            statusName = "Lack Of Work Time",
                            reason = null,
                            linkFile = null
                        });
                    }
                }
            }

            return result;
        }

        public List<RequestWorkTimeDTO> GetAllRequestWorkTime(string? nameSearch, int? status, string? month)
        {
            var result = new List<RequestWorkTimeDTO>();
            var list = _dbContext.Requests
                .Include(r => r.RequestWorkTime)
                .ThenInclude(rw => rw.WorkslotEmployee)
                .ThenInclude(we => we.Workslot)
                .Where(r => r.IsDeleted == false)
                .Where(r => r.requestType == RequestType.WorkTime);

            if (status != -1) list = list.Where(r => (int)r.Status == status);
            var dateFilter = DateTime.ParseExact(month, "yyyy/MM/dd", CultureInfo.InvariantCulture);

            list.Where(r => r.RequestWorkTime.DateOfSlot.Value.Month == dateFilter.Month && r.RequestWorkTime.DateOfSlot.Value.Year == dateFilter.Year).ToList().ForEach(r =>
            {
                var employee = _dbContext.Employees.Where(e => e.IsDeleted == false && e.Id == r.EmployeeSendRequestId).FirstOrDefault();
                var allHourWT = _dbContext.Requests.Include(r => r.RequestWorkTime).Where(r => r.EmployeeSendRequestId == employee.Id && r.Status == RequestStatus.Approved).Select(w => w.RequestWorkTime);
                var timeInMonth = allHourWT.Where(r => r.DateOfSlot.Value.Month == dateFilter.Month && r.DateOfSlot.Value.Year == dateFilter.Year).Count();
                var timeInYear = allHourWT.Where(r => r.DateOfSlot.Value.Year == dateFilter.Year).Count();
                result.Add(new RequestWorkTimeDTO()
                {
                    Id = r.Id,
                    employeeId = employee.Id,
                    employeeName = employee.FirstName + " " + employee.LastName,
                    RealHourStart = r.RequestWorkTime.RealHourStart,
                    RealHourEnd = r.RequestWorkTime.RealHourEnd,
                    NumberOfComeLateHour = r.RequestWorkTime.NumberOfComeLateHour,
                    NumberOfLeaveEarlyHour = r.RequestWorkTime.NumberOfLeaveEarlyHour,
                    TimeInMonth = timeInMonth,
                    TimeInYear = timeInYear,
                    WorkslotEmployeeId = r.RequestWorkTime.WorkslotEmployeeId,
                    SlotStart = r.RequestWorkTime.WorkslotEmployee.Workslot.FromHour,
                    SlotEnd = r.RequestWorkTime.WorkslotEmployee.Workslot.ToHour,
                    DateOfWorkTime = r.RequestWorkTime.DateOfSlot?.ToString("yyyy/MM/dd"),
                    linkFile = r.PathAttachmentFile,
                    Name = r.RequestWorkTime.Name,
                    submitDate = r.SubmitedDate.ToString("yyyy/MM/dd") ?? "",
                    status = (int)r.Status,
                    statusName = r.Status.ToString(),
                    reason = r.Reason,
                    IsDeleted = r.RequestWorkTime.IsDeleted
                });
            });

            if (nameSearch != null)
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
                                          .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                return new { message = "Request not found" };
            }

            // Update the Request status to Approve
            request.Status = RequestStatus.Approved;

            // Step 2: Find all WorkslotEmployees that should be updated
            var dateOfTime = request.RequestWorkTime.DateOfSlot;

            var workslotEmployees = await _dbContext.WorkslotEmployees
                                                    .Include(we => we.Workslot)
                                                    .Where(we => we.EmployeeId == request.EmployeeSendRequestId)
                                                    .Where(we => we.Workslot.DateOfSlot == dateOfTime)
                                                    .ToListAsync();

            // Step 3: Update the AttendanceStatus for these WorkslotEmployees
            var newAttendanceStatus = await _dbContext.AttendanceStatuses
                                                      .Include(att => att.WorkingStatus)
                                                      .FirstOrDefaultAsync(att => att.WorkingStatus != null && att.WorkingStatus.Name == "Worked");

            if (newAttendanceStatus == null)
            {
                return new { message = "Attendance status for the WorkingStatus 'Worked' not found" };
            }

            foreach (var workslotEmployee in workslotEmployees)
            {
                workslotEmployee.AttendanceStatus = newAttendanceStatus;
                workslotEmployee.AttendanceStatusId = newAttendanceStatus.Id;
            }

            // Step 4: Save changes to the database
            await _dbContext.SaveChangesAsync();

            return new { message = "RequestWorkTime approved and WorkslotEmployee updated successfully" };
        }


    }
}
