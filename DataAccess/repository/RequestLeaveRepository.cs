using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.Data.SqlClient.Server;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace DataAccess.Repository
{
    public class RequestLeaveRepository : Repository<RequestLeave>, IRequestLeaveRepository
    {
        private readonly MyDbContext _dbContext;
        private readonly IEmployeeRepository _employeeRepository;

        public RequestLeaveRepository(MyDbContext context, IEmployeeRepository employeeRepository) : base(context)
        {
            // You can add more specific methods here if needed
            _dbContext = context;
            _employeeRepository = employeeRepository;
        }

        public async Task<List<RequestLeaveDTO>> GetAllAsync()
        {
            var ass = await base.GetAllAsync();
            return await ass.Select(a => new RequestLeaveDTO
            {
                Id = a.Id,
                Name = a.Name,
                LeaveTypeId = a.LeaveTypeId,
                IsDeleted = a.IsDeleted
            }).ToListAsync();
        }

        public async Task<bool> AddAsync(RequestLeaveDTO a)
        {
            try
            {
                await base.AddAsync(new RequestLeave() // have dbSaveChange inside method
                {
                    Id = a.Id,
                    Name = a.Name,
                    LeaveTypeId = a.LeaveTypeId,
                    IsDeleted = a.IsDeleted
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

        public object GetRequestLeaveOfEmployeeById(Guid employeeId)
        {

            var employee = _dbContext.Employees.Where(e => e.IsDeleted == false && e.Id == employeeId).FirstOrDefault();
            var result = new List<LeaveRequestDTO>();
            var list = _dbContext.Requests.Include(r => r.RequestLeave).ThenInclude(rl => rl.LeaveType).Include(r => r.RequestLeave).ThenInclude(rl => rl.WorkslotEmployees).ThenInclude(we => we.Workslot).Where(r => r.IsDeleted == false)
                 .Where(r => r.EmployeeSendRequestId == employeeId && r.requestType == RequestType.Leave)
                 .ToList();
            list.ForEach(r =>
            {
                List<DateRangeDTO> dateRange = null;
                dateRange = r.RequestLeave.WorkslotEmployees.Where(r => r.IsDeleted == false).Select(we => new DateRangeDTO() { title = we.Workslot.DateOfSlot.ToString("yyyy/MM/dd"), type = we.Workslot.IsMorning ? "Morning" : "Afternoon" }).ToList().OrderBy(s => DateTime.ParseExact(s.title, "yyyy/MM/dd", CultureInfo.InvariantCulture)).ToList();
                dateRange = MergeToFullDay(dateRange);
                result.Add(new LeaveRequestDTO()
                {
                    id = r.Id,
                    employeeName = employee.FirstName + " " + employee.LastName,
                    startDate = r.RequestLeave.FromDate.ToString("yyyy/MM/dd"),
                    endDate = r.RequestLeave.ToDate.ToString("yyyy/MM/dd"),
                    leaveTypeId = r.RequestLeave.LeaveTypeId,
                    leaveType = r.RequestLeave.LeaveType.Name,
                    status = (int)r.Status,
                    statusName = r.Status.ToString(),
                    reason = r.Reason,
                    linkFile = r.PathAttachmentFile,
                    dateRange = dateRange,
                });
            });

            return result;
        }

        public object GetRequestLeaveAllEmployee(string? nameSearch, int status)
        {
            var result = new List<LeaveRequestDTO>();
            var list = _dbContext.Requests
                .Include(r => r.RequestLeave)
                .ThenInclude(rl => rl.LeaveType)
                .Include(r => r.RequestLeave)
                .ThenInclude(rl => rl.WorkslotEmployees)
                .ThenInclude(we => we.Workslot)
                .Where(r => r.IsDeleted == false)
                .Where(r => r.requestType == RequestType.Leave);
            if (status != -1) list = list.Where(r => (int)r.Status == status);
            list.ToList().ForEach(r =>
            {
                var employee = _dbContext.Employees.Where(e => e.IsDeleted == false && e.Id == r.EmployeeSendRequestId).FirstOrDefault();
                if (employee == null) return;
                List<DateRangeDTO> dateRange = null;
                dateRange = r.RequestLeave.WorkslotEmployees.Where(r => r.IsDeleted == false).Select(we => new DateRangeDTO() { title = we.Workslot.DateOfSlot.ToString("yyyy/MM/dd"), type = we.Workslot.IsMorning ? "Morning" : "Afternoon" }).ToList().OrderBy(s => DateTime.ParseExact(s.title, "yyyy/MM/dd", CultureInfo.InvariantCulture)).ToList();
                dateRange = MergeToFullDay(dateRange);
                result.Add(new LeaveRequestDTO()
                {
                    id = r.Id,
                    employeeId = employee.Id,
                    employeeName = employee.FirstName + " " + employee.LastName,
                    submitDate = r.SubmitedDate.ToString("yyyy/MM/dd"),
                    startDate = r.RequestLeave.FromDate.ToString("yyyy/MM/dd"),
                    endDate = r.RequestLeave.ToDate.ToString("yyyy/MM/dd"),
                    leaveTypeId = r.RequestLeave.LeaveTypeId,
                    leaveType = r.RequestLeave.LeaveType.Name,
                    status = (int)r.Status,
                    statusName = r.Status.ToString(),
                    reason = r.Reason,
                    linkFile = r.PathAttachmentFile,
                    numberOfLeaveDate = dateRange.Count(),
                });
            });

            if (nameSearch != null)
            {
                result = result.Where(r => r.employeeName.ToLower().Contains(nameSearch.ToLower())).ToList();
            }

            return result;
        }

        public async Task<bool> EditRequestLeave(LeaveRequestDTO dto, Guid employeeId)
        {
            // Find the existing request leave by its ID
            var existingRequest = _dbContext.Requests
                                            .Include(r => r.RequestLeave)
                                            .ThenInclude(rl => rl.LeaveType)
                                            .Include(r => r.RequestLeave)
                                            .ThenInclude(rl => rl.WorkslotEmployees)
                                            .FirstOrDefault(r => r.Id == dto.id && r.IsDeleted == false);

            // If no matching request leave is found, return false
            if (existingRequest == null)
            {
                return false;
            }

            // Update fields based on the provided DTO
            if (dto.startDate != null)
            {
                existingRequest.RequestLeave.FromDate = DateTime.ParseExact(dto.startDate, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            }

            if (dto.endDate != null)
            {
                existingRequest.RequestLeave.ToDate = DateTime.ParseExact(dto.endDate, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            }

            if (dto.leaveTypeId != null)
            {
                existingRequest.RequestLeave.LeaveTypeId = (Guid)dto.leaveTypeId;
                var lt = _dbContext.LeaveTypes.Where(lt => lt.IsDeleted == false && lt.Id == dto.leaveTypeId).FirstOrDefault();
                existingRequest.RequestLeave.LeaveType = lt;
            }

            if (dto.status.HasValue)
            {
                if (dto.status.Value == 0)
                {
                    existingRequest.Status = RequestStatus.Pending;
                }
                else
                if (dto.status.Value == 1)
                {
                    existingRequest.Status = RequestStatus.Approved;
                }
                else
                {
                    existingRequest.Status = RequestStatus.Rejected;
                }
            }

            if (dto.reason != null)
            {
                existingRequest.Reason = dto.reason;
            }

            if (dto.linkFile != null)
            {
                existingRequest.PathAttachmentFile = dto.linkFile;
            }
            //var employee = _dbContext.Employees.Include(e => e.Department).ThenInclude(d => d.WorkTrackSetting).ThenInclude(wts => wts.WorkDateSetting).Where(e => e.Id == employeeId).FirstOrDefault();
            // Update the WorkslotEmployees if needed. This assumes that the WorkslotEmployees are uniquely identified by their 'title' (date) and 'type' (morning/afternoon)
            List<WorkslotEmployee> newItems = new List<WorkslotEmployee>();

            // Get the current list of WorkslotEmployees that are not deleted.
            List<WorkslotEmployee> currentItems = existingRequest.RequestLeave.WorkslotEmployees.Where(we => we.IsDeleted == false).ToList();

            //if (dto.dateRange != null)
            //{
            //    HandleFullday(dto.dateRange);
            //    // Add all new WorkslotEmployees based on the new dateRange
            //    foreach (var dateRange in dto.dateRange)
            //    {
            //        var workslotToAdd = _dbContext.WorkslotEmployees.Include(we => we.Workslot)
            //                                      .Where(we => we.IsDeleted == false)
            //                                      .Where(we => we.EmployeeId == employeeId);
            //        //foreach (var workslot in workslotToAdd)
            //        //{
            //        //    var test = string.Format("{0:yyyy/MM/dd}", workslot.Workslot.DateOfSlot) == dateRange.title;
            //        //}
            //        var workslotToAdd2 = workslotToAdd.Where(we => we.Workslot.DateOfSlot.Date == DateTime.ParseExact(dateRange.title, "yyyy/MM/dd", CultureInfo.InvariantCulture).Date).Where(we => (we.Workslot.IsMorning ? "Morning" : "Afternoon") == dateRange.type).FirstOrDefault();

            //        if (workslotToAdd2 != null)
            //        {
            //            //existingRequest.RequestLeave.WorkslotEmployees.Add(workslotToAdd2);
            //            newItems.Add(workslotToAdd2);  // Add to newItems list.
            //        }
            //        else
            //        {
            //            var workSlot = _dbContext.Workslots.FirstOrDefault(we => we.DateOfSlot.Date == DateTime.ParseExact(dateRange.title, "yyyy/MM/dd", CultureInfo.InvariantCulture).Date && (we.IsMorning ? "Morning" : "Afternoon") == dateRange.type);
            //            var worktrackSetting = employee.Department.WorkTrackSetting;
            //            if (workSlot == null)
            //            {
            //                workSlot = new Workslot()
            //                {
            //                    Id = Guid.NewGuid(),
            //                    Name = DateTime.ParseExact(dateRange.title, "yyyy/MM/dd", CultureInfo.InvariantCulture).DayOfWeek.ToString(),
            //                    IsMorning = dateRange.type == "Morning" ? true : false,
            //                    DateOfSlot = DateTime.ParseExact(dateRange.title, "yyyy/MM/dd", CultureInfo.InvariantCulture),
            //                    WorkTrackId = worktrackSetting.Id,
            //                    IsDeleted = false
            //                };
            //                await _dbContext.Workslots.AddAsync(workSlot);
            //            }

            //            var newWorkslotEmployee = new WorkslotEmployee()
            //            {
            //                Id = Guid.NewGuid(),
            //                EmployeeId = employeeId,
            //                Employee = employee,
            //                Workslot = workSlot,
            //                WorkslotId = workSlot.Id,
            //                AttendanceStatusId = _dbContext.AttendanceStatuses.FirstOrDefault(att => att.LeaveTypeId != null && att.LeaveTypeId == existingRequest.RequestLeave.LeaveTypeId).Id,
            //                AttendanceStatus = _dbContext.AttendanceStatuses.FirstOrDefault(att => att.LeaveTypeId != null && att.LeaveTypeId == existingRequest.RequestLeave.LeaveTypeId),
            //                IsDeleted = false
            //            };
            //            await _dbContext.WorkslotEmployees.AddAsync(newWorkslotEmployee);
            //            newItems.Add(newWorkslotEmployee);
            //        }
            //    }

            //    var itemsToRemove = currentItems.Except(newItems).ToList();

            //    // Remove items.
            //    foreach (var itemToRemove in itemsToRemove)
            //    {
            //        existingRequest.RequestLeave.WorkslotEmployees.Remove(itemToRemove);
            //        var attendanceStatus = _dbContext.AttendanceStatuses.Include(att => att.WorkingStatus).FirstOrDefault(att => att.WorkingStatus.Name == "Not Work Yet");
            //        if (attendanceStatus == null)
            //        {
            //            attendanceStatus = new AttendanceStatus()
            //            {
            //                Id = Guid.NewGuid(),
            //                IsDeleted = false,
            //                WorkingStatus = _dbContext.WorkingStatuses.Where(ws => ws.Name == "Not Work Yet").FirstOrDefault(),
            //                WorkingStatusId = _dbContext.WorkingStatuses.Where(ws => ws.Name == "Not Work Yet").FirstOrDefault().Id
            //            };
            //        }
            //        itemToRemove.AttendanceStatus = attendanceStatus;
            //        itemToRemove.AttendanceStatusId = attendanceStatus.Id;
            //    }

            //    // Add new items.
            //    foreach (var newItem in newItems)
            //    {
            //        if (!currentItems.Contains(newItem))
            //        {
            //            var attendanceStatus = _dbContext.AttendanceStatuses.Include(att => att.LeaveType).FirstOrDefault(att => att.LeaveTypeId == dto.leaveTypeId);
            //            if (attendanceStatus == null)
            //            {
            //                attendanceStatus = new AttendanceStatus()
            //                {
            //                    Id = Guid.NewGuid(),
            //                    IsDeleted = false,
            //                    LeaveType = _dbContext.LeaveTypes.Where(lt => lt.Id == existingRequest.RequestLeave.LeaveTypeId).FirstOrDefault(),
            //                    LeaveTypeId = existingRequest.RequestLeave.LeaveTypeId
            //                };
            //            }
            //            newItem.AttendanceStatus = attendanceStatus;
            //            newItem.AttendanceStatusId = attendanceStatus.Id;
            //            existingRequest.RequestLeave.WorkslotEmployees.Add(newItem);
            //        }
            //    }
            //}

            // Save changes to the database
            await _dbContext.SaveChangesAsync();

            return true;
        }


        public async Task<WorkDateSettingDTO> GetWorkDateSettingFromEmployeeId(Guid employeeId)
        {
            var employee = _dbContext.Employees.Include(e => e.Department).ThenInclude(d => d.WorkTrackSetting).ThenInclude(wts => wts.WorkDateSetting).Where(e => e.Id == employeeId).FirstOrDefault();
            if (employee != null)
            {
                var workdatesetting = employee.Department.WorkTrackSetting.WorkDateSetting;
                if (workdatesetting != null)
                {
                    return new WorkDateSettingDTO()
                    {
                        Id = workdatesetting.Id,
                        DateStatus = JsonSerializer.Deserialize<DateStatusDTO>(workdatesetting.DateStatus),
                        IsDeleted = workdatesetting.IsDeleted
                    };
                }
                else
                {
                    return null;
                }

            }
            else
            {
                throw new Exception("Employee Not Exist");
            }
        }

        public async Task<object> CreateRequestLeave(LeaveRequestDTO dto, Guid employeeId)
        {
            RequestLeave newRequestLeave = new RequestLeave()
            {
                Id = Guid.NewGuid(),
                FromDate = DateTime.ParseExact(dto.startDate, "yyyy/MM/dd", CultureInfo.InvariantCulture),
                ToDate = DateTime.ParseExact(dto.endDate, "yyyy/MM/dd", CultureInfo.InvariantCulture),
                LeaveTypeId = (Guid)dto.leaveTypeId,
                IsDeleted = false,
                Name = dto.Name ?? ""
            };

            // Step 1: Create a new Request and populate its fields
            Request newRequest = new Request()
            {
                Id = Guid.NewGuid(),
                EmployeeSendRequestId = employeeId,
                Status = RequestStatus.Pending,  // default status
                IsDeleted = false,
                RequestLeaveId = newRequestLeave.Id,
                RequestLeave = newRequestLeave,
                Message = "",
                PathAttachmentFile = dto.linkFile ?? "",
                Reason = dto.reason ?? "",
                SubmitedDate = DateTime.Now,
                requestType = RequestType.Leave,

            };

            List<WorkslotEmployee> newWorkslotEmployees = new List<WorkslotEmployee>();
            var employee = _dbContext.Employees.Include(e => e.Department).ThenInclude(d => d.WorkTrackSetting).ThenInclude(wts => wts.WorkDateSetting).Where(e => e.Id == employeeId).FirstOrDefault();
            // Step 3: Add WorkslotEmployees based on dateRange
            if (dto.dateRange != null)
            {
                HandleFullday(dto.dateRange);
                foreach (var dateRange in dto.dateRange)
                {
                    // Similar logic as in EditRequestLeave for adding WorkslotEmployees
                    var workslotToAdd = _dbContext.WorkslotEmployees.Include(we => we.Workslot)
                                                .Where(we => we.IsDeleted == false)
                                                .Where(we => we.EmployeeId == employeeId)
                                                .Where(we => we.Workslot.DateOfSlot.Date == DateTime.ParseExact(dateRange.title, "yyyy/MM/dd", CultureInfo.InvariantCulture).Date)
                                                .Where(we => (we.Workslot.IsMorning ? "Morning" : "Afternoon") == dateRange.type)
                                                .FirstOrDefault();

                    if (workslotToAdd != null)
                    {
                        newWorkslotEmployees.Add(workslotToAdd);
                    }
                    else
                    {
                        //var workSlot = _dbContext.Workslots.FirstOrDefault(we => we.DateOfSlot.Date == DateTime.ParseExact(dateRange.title, "yyyy/MM/dd", CultureInfo.InvariantCulture).Date && (we.IsMorning ? "Morning" : "Afternoon") == dateRange.type);
                        //var worktrackSetting = employee.Department.WorkTrackSetting;
                        //if (workSlot == null)
                        //{
                        //    workSlot = new Workslot()
                        //    {
                        //        Id = Guid.NewGuid(),
                        //        Name = DateTime.ParseExact(dateRange.title, "yyyy/MM/dd", CultureInfo.InvariantCulture).DayOfWeek.ToString(),
                        //        IsMorning = dateRange.type == "Morning" ? true : false,
                        //        DateOfSlot = DateTime.ParseExact(dateRange.title, "yyyy/MM/dd", CultureInfo.InvariantCulture),
                        //        IsDeleted = false
                        //    };
                        //    await _dbContext.Workslots.AddAsync(workSlot);
                        //}

                        //var newWorkslotEmployee = new WorkslotEmployee()
                        //{
                        //    Id = Guid.NewGuid(),
                        //    EmployeeId = employeeId,
                        //    Employee = employee,
                        //    Workslot = workSlot,
                        //    WorkslotId = workSlot.Id,
                        //    AttendanceStatusId = _dbContext.AttendanceStatuses.FirstOrDefault(att => att.LeaveTypeId != null && att.LeaveTypeId == dto.leaveTypeId).Id,
                        //    AttendanceStatus = _dbContext.AttendanceStatuses.FirstOrDefault(att => att.LeaveTypeId != null && att.LeaveTypeId == dto.leaveTypeId),
                        //    IsDeleted = false
                        //};

                        //await _dbContext.WorkslotEmployees.AddAsync(newWorkslotEmployee);
                        //newWorkslotEmployees.Add(newWorkslotEmployee);
                    }
                }
                newRequest.RequestLeave.WorkslotEmployees = new List<WorkslotEmployee>();
                foreach (var newItem in newWorkslotEmployees)
                {
                    //var attendanceStatus = _dbContext.AttendanceStatuses.Include(att => att.LeaveType).FirstOrDefault(att => att.LeaveTypeId == dto.leaveTypeId);
                    //if (attendanceStatus == null)
                    //{
                    //    attendanceStatus = new AttendanceStatus()
                    //    {
                    //        Id = Guid.NewGuid(),
                    //        IsDeleted = false,
                    //        LeaveType = _dbContext.LeaveTypes.Where(lt => lt.Id == dto.leaveTypeId).FirstOrDefault(),
                    //        LeaveTypeId = dto.leaveTypeId
                    //    };
                    //    await _dbContext.AttendanceStatuses.AddAsync(attendanceStatus);
                    //}
                    //newItem.AttendanceStatus = attendanceStatus;
                    //newItem.AttendanceStatusId = attendanceStatus.Id;
                    newRequest.RequestLeave.WorkslotEmployees.Add(newItem);
                }
            }

            await _dbContext.RequestLeaves.AddAsync(newRequestLeave);
            await _dbContext.Requests.AddAsync(newRequest);
            // Step 4: Save to the database
            await _dbContext.SaveChangesAsync();

            return new
            {
                newRequestLeaveId = newRequestLeave.Id,
                newRequestId = newRequest.Id
            };
        }

        public void HandleFullday(List<DateRangeDTO> dateRange)
        {
            List<DateRangeDTO> newDateRange = new List<DateRangeDTO>();
            HashSet<string> existingEntries = new HashSet<string>();

            foreach (var entry in dateRange)
            {
                string key = entry.title + "-" + entry.type;

                // Skip this entry if it already exists
                if (existingEntries.Contains(key))
                {
                    continue;
                }

                // Add to existing entries
                existingEntries.Add(key);

                if (entry.type == "Full Day")
                {
                    // Split Fullday into Morning and Afternoon, ensuring there are no duplicates
                    string morningKey = entry.title + "-Morning";
                    string afternoonKey = entry.title + "-Afternoon";

                    if (!existingEntries.Contains(morningKey))
                    {
                        newDateRange.Add(new DateRangeDTO() { title = entry.title, type = "Morning" });
                        existingEntries.Add(morningKey);
                    }

                    if (!existingEntries.Contains(afternoonKey))
                    {
                        newDateRange.Add(new DateRangeDTO() { title = entry.title, type = "Afternoon" });
                        existingEntries.Add(afternoonKey);
                    }
                }
                else
                {
                    newDateRange.Add(entry);
                }
            }

            // Replace the original dateRange with the new one
            dateRange.Clear();
            dateRange.AddRange(newDateRange);
        }

        public static List<DateRangeDTO> MergeToFullDay(List<DateRangeDTO> dateRanges)
        {
            Dictionary<string, string> dateTypeMap = new Dictionary<string, string>();
            List<DateRangeDTO> mergedDateRanges = new List<DateRangeDTO>();

            foreach (var range in dateRanges)
            {
                if (range.title == null || range.type == null)
                {
                    continue; // Skip any null entries.
                }

                if (dateTypeMap.ContainsKey(range.title))
                {
                    dateTypeMap[range.title] = "Full Day";
                }
                else
                {
                    dateTypeMap[range.title] = range.type;
                }
            }

            foreach (var entry in dateTypeMap)
            {
                mergedDateRanges.Add(new DateRangeDTO { title = entry.Key, type = entry.Value });
            }

            return mergedDateRanges;
        }

        public async Task<object> ApproveRequestAndChangeWorkslotEmployee(Guid requestId)
        {
            // Step 1: Retrieve the Request by requestId
            var request = await _dbContext.Requests
                                          .Include(r => r.RequestLeave)
                                          .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                return new { message = "Request not found" };
            }

            // Update the Request status to Approve
            request.Status = RequestStatus.Approved;

            // Step 2: Find all WorkslotEmployees that should be updated
            var fromDate = request.RequestLeave.FromDate;
            var toDate = request.RequestLeave.ToDate;
            var leaveTypeId = request.RequestLeave.LeaveTypeId;

            var workslotEmployees = await _dbContext.WorkslotEmployees
                                                    .Include(we => we.Workslot)
                                                    .Where(we => we.EmployeeId == request.EmployeeSendRequestId)
                                                    .Where(we => we.Workslot.DateOfSlot >= fromDate && we.Workslot.DateOfSlot <= toDate)
                                                    .ToListAsync();

            // Step 3: Update the AttendanceStatus for these WorkslotEmployees
            var newAttendanceStatus = await _dbContext.AttendanceStatuses
                                                      .FirstOrDefaultAsync(att => att.LeaveTypeId == leaveTypeId);

            if (newAttendanceStatus == null)
            {
                return new { message = "Attendance status for the given LeaveTypeId not found" };
            }

            foreach (var workslotEmployee in workslotEmployees)
            {
                workslotEmployee.AttendanceStatus = newAttendanceStatus;
                workslotEmployee.AttendanceStatusId = newAttendanceStatus.Id;
            }

            // Step 4: Save changes to the database
            await _dbContext.SaveChangesAsync();

            return new { message = "Request approved and WorkslotEmployee updated successfully" };
        }

        public object GetRequestLeaveByRequestId(Guid requestId)
        {
            // Find the request by its Id
            var request = _dbContext.Requests
                .Include(r => r.RequestLeave)
                    .ThenInclude(rl => rl.LeaveType)
                .Include(r => r.RequestLeave)
                    .ThenInclude(rl => rl.WorkslotEmployees)
                    .ThenInclude(we => we.Workslot)
                .Where(r => r.IsDeleted == false && r.Id == requestId && r.requestType == RequestType.Leave)
                .FirstOrDefault();

            if (request == null)
            {
                return null; // Or any other error handling
            }

            // Find the employee details
            var employee = _dbContext.Employees.Where(e => e.IsDeleted == false && e.Id == request.EmployeeSendRequestId).FirstOrDefault();

            List<DateRangeDTO> dateRange = request.RequestLeave.WorkslotEmployees
                .Where(r => r.IsDeleted == false)
                .Select(we => new DateRangeDTO() { title = we.Workslot.DateOfSlot.ToString("yyyy/MM/dd"), type = we.Workslot.IsMorning ? "Morning" : "Afternoon" })
                .OrderBy(s => DateTime.ParseExact(s.title, "yyyy/MM/dd", CultureInfo.InvariantCulture))
                .ToList();

            dateRange = MergeToFullDay(dateRange);

            var result = new LeaveRequestDTO()
            {
                id = request.Id,
                employeeName = employee.FirstName + " " + employee.LastName,
                startDate = request.RequestLeave.FromDate.ToString("yyyy/MM/dd"),
                endDate = request.RequestLeave.ToDate.ToString("yyyy/MM/dd"),
                leaveTypeId = request.RequestLeave.LeaveTypeId,
                leaveType = request.RequestLeave.LeaveType.Name,
                status = (int)request.Status,
                statusName = request.Status.ToString(),
                reason = request.Reason,
                linkFile = request.PathAttachmentFile,
                dateRange = dateRange,
            };

            return result;
        }


    }
}
