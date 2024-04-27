using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using System.Text;
using Microsoft.VisualBasic;

namespace DataAccess.Repository
{
    public class RequestOverTimeRepository : Repository<RequestOverTime>, IRequestOverTimeRepository
    {
        private readonly MyDbContext _dbContext;

        public RequestOverTimeRepository(MyDbContext context) : base(context)
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

        public async Task<List<RequestOverTimeDTO>> GetAllAsync()
        {
            var ass = await base.GetAllAsync();
            return await ass.Select(a => new RequestOverTimeDTO
            {
                id = a.Id,
                Name = a.Name,
                timeStart = a.FromHour.Hour.ToString(),
                NumberOfHour = a.NumberOfHour,
                IsDeleted = a.IsDeleted
            }).ToListAsync();
        }

        public object GetRequestOverTimeOfEmployeeById(Guid employeeId)
        {
            var employee = _dbContext.Employees.Where(e => e.IsDeleted == false && e.Id == employeeId).FirstOrDefault();
            if (employee == null)
            {
                throw new Exception("Employee not found");
            }
            var result = new List<object>();
            var list = _dbContext.Requests.Include(r => r.RequestOverTime).Where(r => r.IsDeleted == false)
                 .Where(r => r.EmployeeSendRequestId == employeeId && r.requestType == RequestType.OverTime)
                 .ToList();
            list.ForEach(r =>
            {
                result.Add(new RequestOverTimeDTO()
                {
                    id = r.Id,
                    employeeId = employeeId,
                    employeeName = employee.FirstName + " " + employee.LastName,
                    employeeNumber = employee.EmployeeNumber,
                    RequestOverTimeId = r.RequestOverTimeId,
                    Name = r.RequestOverTime?.Name ?? "",
                    Date = r.RequestOverTime.DateOfOverTime.ToString("yyyy/MM/dd"),
                    timeStart = r.RequestOverTime.FromHour.ToString("HH:mm"),
                    NumberOfHour = r.RequestOverTime.NumberOfHour,
                    timeEnd = r.RequestOverTime.ToHour.ToString("HH:mm"),
                    statusRequest = r.Status.ToString(),
                    reason = r.Reason,
                    reasonReject = r.Message,
                    linkFile = r.PathAttachmentFile,
                    status = _dbContext.WorkingStatuses.FirstOrDefault(ws => ws.Id == r.RequestOverTime.WorkingStatusId)?.Name ?? "",
                    workingStatusId = r.RequestOverTime.WorkingStatusId,
                    deciderId = r.EmployeeIdLastDecider,
                    IsDeleted = r.IsDeleted
                });
            });

            return result;
        }

        public async Task<object> CreateRequestOvertime(CreateRequestOverTimeDTO dto, Guid employeeId)
        {
            // Check for null or invalid DTO fields
            if (dto.timeStart == null || dto.timeEnd == null || dto.Date == null || dto.reason == null)
            {
                throw new Exception("lack of 1 in 4 field: timeStart, NumberOfHour, Date, reason");
            }

            // Parse times
            var startTime = DateTime.ParseExact(dto.timeStart, "HH:mm", CultureInfo.InvariantCulture);
            var endTime = DateTime.ParseExact(dto.timeEnd, "HH:mm", CultureInfo.InvariantCulture);

            // Check if start time is earlier than end time
            if (startTime >= endTime)
            {
                throw new Exception("Start time must be earlier than end time.");
            }

            // Calculate duration and validate it
            var duration = (endTime - startTime).TotalHours;
            if (duration < 1 || duration > 4)
            {
                throw new Exception("Duration must be at least 1 hours and no more than 4 hours.");
            }

            var departmentId = _dbContext.Employees.FirstOrDefault(e => e.Id == employeeId)?.DepartmentId;
            if (departmentId == null) throw new Exception("The Employee not in any Department can not create OverTime request");

            var dtoDateParsed = DateTime.ParseExact(dto.Date, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            var workSlots = _dbContext.Workslots
            .Where(ws => ws.DepartmentId == departmentId && ws.DateOfSlot.Date == dtoDateParsed.Date)
            .ToList();
            DateTime slotStartTime = DateTime.Now;
            DateTime slotEndTime = DateTime.Now;
            bool shouldCheckOverlap1 = false;
            bool shouldCheckOverlap2 = false;
            // Check each work slot for a time overlap
            foreach (var slot in workSlots)
            {
                if (slot.IsMorning)
                {
                    slotStartTime = DateTime.ParseExact(slot.FromHour, "HH:mm", CultureInfo.InvariantCulture);
                    shouldCheckOverlap1 = true;
                }
                
                if (!slot.IsMorning)
                {
                    slotEndTime = DateTime.ParseExact(slot.ToHour, "HH:mm", CultureInfo.InvariantCulture);
                    shouldCheckOverlap2 = true;
                }

            }
            
            if (shouldCheckOverlap1 && shouldCheckOverlap2 && CheckForTimeOverlap(slotStartTime, slotEndTime, startTime, endTime))
            {
                throw new Exception("the OT time overlap the worktime period");
            }

            var workingStatus = _dbContext.WorkingStatuses.FirstOrDefault(ws => ws.Id == dto.workingStatusId);
            if (workingStatus == null)
            {
                workingStatus = _dbContext.WorkingStatuses.FirstOrDefault(ws => ws.Name == "Not Work Yet");
            }

            RequestOverTime newRequestOverTime = new RequestOverTime()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? "",
                DateOfOverTime = DateTime.ParseExact(dto.Date, "yyyy/MM/dd", CultureInfo.InvariantCulture),
                FromHour = DateTime.ParseExact(dto.timeStart, "HH:mm", CultureInfo.InvariantCulture),
                ToHour = DateTime.ParseExact(dto.timeEnd, "HH:mm", CultureInfo.InvariantCulture),
                WorkingStatus = workingStatus,
                WorkingStatusId = workingStatus.Id,
                NumberOfHour = (DateTime.ParseExact(dto.timeEnd, "HH:mm", CultureInfo.InvariantCulture) - DateTime.ParseExact(dto.timeStart, "HH:mm", CultureInfo.InvariantCulture)).TotalHours,
                IsDeleted = false  // Set the soft delete flag to false
            };

            await _dbContext.RequestOverTimes.AddAsync(newRequestOverTime);

            Employee employeeSendRequest = _dbContext.Employees.Include(e => e.Department).FirstOrDefault(e => e.Id == employeeId);
            if (employeeSendRequest == null)
            {
                throw new Exception("Employee Send Request Not Found");
            }

            // Assuming employeeSendRequest is an object of type Employee
            //var departmentId = employeeSendRequest.DepartmentId;
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

            Guid newRequestId = Guid.NewGuid();
            // Initialize new Request and RequestOvertime objects
            Request newRequest = new Request()
            {
                Id = newRequestId,
                EmployeeSendRequestId = employeeId,
                EmployeeSendRequest = employeeSendRequest,
                Status = RequestStatus.Pending,  // default status
                IsDeleted = false,
                RequestOverTimeId = newRequestOverTime.Id,
                RequestOverTime = newRequestOverTime,
                Message = "",
                PathAttachmentFile = dto.linkFile ?? "",
                Reason = dto.reason ?? "",
                SubmitedDate = DateTime.Now,
                requestType = RequestType.OverTime,
                EmployeeIdLastDecider = managerId
            };

            // Handle date-specific logic if necessary
            // Since there is no Workslot equivalent for Overtime, we may handle dates differently
            // ...

            // Add the new Request and RequestOverTime to the database and save changes
            
            await _dbContext.Requests.AddAsync(newRequest);
            await _dbContext.SaveChangesAsync();
            await SendRequestOvertimeToManagerFirebase(newRequestId);

            return new
            {
                RequestOverTimeId = newRequestOverTime.Id,
                RequestId = newRequest.Id
            };
        }

        public async Task<object> DeleteOvertimeRequest(Guid requestId)
        {
            try
            {
                var request = await _dbContext.Requests
                    .Include(r => r.RequestOverTime)
                    .FirstOrDefaultAsync(r => r.Id == requestId && r.requestType == RequestType.OverTime);

                if (request == null)
                {
                    throw new Exception("Overtime request not found.");
                }

                // Mark the RequestOverTime as deleted
                if (request.RequestOverTime != null)
                {
                    request.RequestOverTime.IsDeleted = true;
                }

                // Mark the Request itself as deleted
                request.IsDeleted = true;

                // Save changes to the database
                await _dbContext.SaveChangesAsync();

                return new { message = "Overtime request deleted successfully." };
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting overtime request: " + ex.Message);
            }
        }

        public bool CheckForTimeOverlap(DateTime startTimeA, DateTime endTimeA, DateTime startTimeB, DateTime endTimeB)
        {
            // Check if the times are valid before comparing them
            if (startTimeA >= endTimeA || startTimeB >= endTimeB)
            {
                // Optional: Handle invalid times differently, perhaps log or alert.
                return false; // Assuming overlapping cannot be determined if times are invalid.
            }

            // Return true if there's an overlap, false otherwise
            return startTimeA < endTimeB && endTimeA > startTimeB;
        }

        public async Task<object> EditRequestOvertimeOfEmployee(EditRequestOverTimeDTO dto, Guid employeeId)
        {
            // Step 1: Retrieve the record from the database using its ID
            Request request = _dbContext.Requests.Include(r => r.RequestOverTime).Where(r => r.IsDeleted == false).Where(r => r.Id == dto.requestId && r.EmployeeSendRequestId == employeeId).FirstOrDefault();
            if (request == null)
            {
                throw new Exception("request Id not found");
            }
            RequestOverTime existingRequestOverTime = request.RequestOverTime;

            // Check if the RequestOverTime exists
            if (existingRequestOverTime == null || request == null)
            {
                throw new Exception("RequestOverTime not found.");
            }

            // Step 2: Update the necessary fields
            if (dto.Date != null)
            {
                existingRequestOverTime.DateOfOverTime = DateTime.ParseExact(dto.Date, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            }

            if (dto.timeStart != null)
            {
                existingRequestOverTime.FromHour = DateTime.ParseExact(dto.timeStart, "HH:mm", CultureInfo.InvariantCulture);
            }

            if (dto.timeEnd != null)
            {
                existingRequestOverTime.ToHour = DateTime.ParseExact(dto.timeEnd, "HH:mm", CultureInfo.InvariantCulture);
            }

            if (dto.Name != null)
            {
                existingRequestOverTime.Name = dto.Name;
            }
            var isRequestChange = false;
            var isStatusChange = false;

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

            if (dto.messageFromDecider != null)
            {
                request.Message = dto.messageFromDecider;
                isRequestChange = true;
            }

            if (dto.status != null)
            {
                request.Status = dto.status == 0 ? RequestStatus.Pending : (dto.status == 1 ? RequestStatus.Approved : RequestStatus.Rejected);
                isRequestChange = true;
                isStatusChange = true;
            }

            if (dto.workingStatusId != null)
            {
                existingRequestOverTime.WorkingStatusId = (Guid)dto.workingStatusId;
                existingRequestOverTime.WorkingStatus = _dbContext.WorkingStatuses.FirstOrDefault(ws => ws.Id == dto.workingStatusId);
            }

            if (dto.isDeleted != null)
            {
                request.IsDeleted = (bool)dto.isDeleted;
                existingRequestOverTime.IsDeleted = (bool)dto.isDeleted;
            }

            // Update NumberOfHour based on new FromHour and ToHour
            existingRequestOverTime.NumberOfHour = (existingRequestOverTime.ToHour - existingRequestOverTime.FromHour).TotalHours;

            // Step 3: Save the changes to the database
            
            //_dbContext.RequestOverTimes.Update(existingRequestOverTime);
            //if (isRequestChange) _dbContext.Requests.Update(request);
            await _dbContext.SaveChangesAsync();
            if (isStatusChange)
            {
                await SendRequestOvertimeToEmployeeFirebase(request.Id);
            }

            return new
            {
                RequestOverTimeId = existingRequestOverTime.Id,
                UpdatedFields = new
                {
                    DateOfOverTime = existingRequestOverTime.DateOfOverTime,
                    FromHour = existingRequestOverTime.FromHour,
                    ToHour = existingRequestOverTime.ToHour
                }
            };
        }

        public List<RequestOverTimeDTO> GetAllRequestOverTime(string? nameSearch, int status, DateTime month, Guid? employeeId = null)
        {
            var result = new List<RequestOverTimeDTO>();
            var list = _dbContext.Requests
                .Include(r => r.RequestOverTime)
                .ThenInclude(ro => ro.WorkingStatus)
                .Where(r => r.IsDeleted == false)
                .Where(r => r.requestType == RequestType.OverTime);

            if (status != -1) list = list.Where(r => (int)r.Status == status);

            // Filter by manager's department if employeeId is provided
            Guid? departmentId = null;
            if (employeeId != null)
            {
                var manager = _dbContext.Employees
                    .Include(e => e.UserAccount)
                    .ThenInclude(ua => ua.Role)
                    .FirstOrDefault(e => e.Id == employeeId);

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

            list.Where(r => r.RequestOverTime.DateOfOverTime.Month == month.Month && r.RequestOverTime.DateOfOverTime.Year == month.Year).ToList().ForEach(r =>
            {
                var employee = _dbContext.Employees.Where(e => e.IsDeleted == false && e.Id == r.EmployeeSendRequestId).FirstOrDefault();
                var allHourOT = _dbContext.Requests.Include(r => r.RequestOverTime).Where(r => r.EmployeeSendRequestId == employee.Id && r.Status == RequestStatus.Approved).Select(w => w.RequestOverTime);
                var timeInMonth = allHourOT.Where(r => r.DateOfOverTime.Month == month.Month && r.DateOfOverTime.Year == month.Year).Sum(r => r.NumberOfHour);
                var timeInYear = allHourOT.Where(r => r.DateOfOverTime.Year == month.Year).Sum(r => r.NumberOfHour);
                result.Add(new RequestOverTimeDTO()
                {
                    id = r.Id,
                    employeeId = employee.Id,
                    employeeName = employee.FirstName + " " + employee.LastName,
                    employeeNumber = employee.EmployeeNumber,
                    RequestOverTimeId = r.RequestOverTimeId,
                    workingStatusId = r.RequestOverTime.WorkingStatusId,
                    timeInMonth = timeInMonth,
                    timeInYear = timeInYear,
                    workingStatus = r.RequestOverTime.WorkingStatus.Name,
                    Date = r.RequestOverTime.DateOfOverTime.ToString("yyyy/MM/dd"),
                    timeStart = r.RequestOverTime.FromHour.ToString("HH:mm"),
                    timeEnd = r.RequestOverTime.ToHour.ToString("HH:mm"),
                    NumberOfHour = r.RequestOverTime.NumberOfHour,
                    submitDate = r.SubmitedDate.ToString("yyyy/MM/dd"),
                    IsDeleted = r.RequestOverTime.IsDeleted,
                    status = r.Status.ToString(),
                    linkFile = r.PathAttachmentFile ?? "",
                    reason = r.Reason,
                    reasonReject = r.Message,
                    deciderId = r.EmployeeIdLastDecider,
                });
            });

            if (nameSearch != null)
            {
                result = result.Where(r => r.employeeName.ToLower().Contains(nameSearch.ToLower())).ToList();
            }

            return result;
        }

        public async Task<object> CancelApprovedOvertimeRequest(RequestReasonDTO requestObj)
        {
            Guid requestId = requestObj.requestId;
            string reason = requestObj.reason;

            // Retrieve the request by requestId
            var request = await _dbContext.Requests
                                           .Include(r => r.RequestOverTime)
                                           .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                throw new Exception("RequestId not found.");
            }

            if (request.RequestOverTime == null)
            {
                throw new Exception("Request OverTime not found.");
            }
            // Check if the overtime date is in the past
            if (request.RequestOverTime.DateOfOverTime.Date < DateTime.Today)
            {
                throw new Exception("Cannot cancel overtime for past dates.");
            }

            // Check if the request is indeed approved; only approved requests can be cancelled
            if (request.Status != RequestStatus.Approved)
            {
                throw new Exception("Only approved overtime requests can be cancelled.");
            }

            // Set the Request status to Cancelled or Rejected based on your application logic
            request.Status = RequestStatus.Cancel; // Assuming Cancelled is a defined status in your system
            request.Message = reason;
            request.EmployeeIdLastDecider = requestObj.employeeIdDecider;

            // Assuming you have a specific logic to handle the cancellation of an overtime request
            // For example, updating related entities or states specific to the overtime process

            // Save the changes to the database
            await _dbContext.SaveChangesAsync();
            await SendRequestOvertimeToEmployeeFirebase(requestId);

            return new { message = "Overtime request cancelled successfully." };
        }

        public async Task<object> DeleteOvertimeRequestIfNotApproved(Guid requestId, Guid? employeeIdDecider)
        {
            // Retrieve the request by its Id
            var request = await _dbContext.Requests
                                           .Include(r => r.RequestOverTime)
                                           .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                throw new Exception("Overtime request not found.");
            }

            // Check if the request is already approved
            if (request.Status == RequestStatus.Approved)
            {
                throw new Exception("Approved Overtime requests cannot be deleted.");
            }

            // Mark the request and request leave as deleted
            request.IsDeleted = true;
            request.EmployeeIdLastDecider = employeeIdDecider;
            if (request.RequestOverTime != null)
            {
                request.RequestOverTime.IsDeleted = true;
            }

            // Save the changes to the database
            await _dbContext.SaveChangesAsync();

            return new { message = "Overtime request Deleted successfully." };
        }

        public async Task<bool> SendRequestOvertimeToManagerFirebase(Guid requestId)
        {
            // Define the path specific to the manager
            string managerPath = "/managerNoti"; // Replace '/managerPath' with the actual path for the manager
                                                 // Call the SendLeaveRequestStatusToFirebase method with the manager path
            return await SendOvertimeRequestStatusToFirebase(requestId, managerPath);
        }

        public async Task<bool> SendRequestOvertimeToEmployeeFirebase(Guid requestId)
        {
            // Define the path specific to the employee
            string employeePath = "/employeeNoti"; // Replace '/employeePath' with the actual path for the employee
                                                   // Call the SendLeaveRequestStatusToFirebase method with the employee path
            return await SendOvertimeRequestStatusToFirebase(requestId, employeePath);
        }

        public async Task<bool> SendOvertimeRequestStatusToFirebase(Guid requestId, string path)
        {
            var request = await _dbContext.Requests
                .Include(r => r.RequestOverTime)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null || request.RequestOverTime == null)
            {
                throw new Exception("Request OverTime of requestId " + requestId + " Not Found");
            }

            var manager = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeIdLastDecider);

            // Generate a reverse timestamp key for sorting in Firebase (newer entries first)
            long reverseTimestamp = DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks;

            var firebaseData = new
            {
                requestId = request.Id,
                employeeSenderId = request.EmployeeSendRequestId,
                employeeSenderName = request.EmployeeSendRequest != null ? request.EmployeeSendRequest.FirstName + " " + request.EmployeeSendRequest.LastName : null,
                employeeSenderNumber = request.EmployeeSendRequest != null ? request.EmployeeSendRequest.EmployeeNumber : null,
                employeeDeciderId = request.EmployeeIdLastDecider,
                employeeDeciderName = manager != null ? manager.FirstName + " " + manager.LastName : null,
                employeeDeciderNumber = manager != null ? manager.EmployeeNumber : null,
                leaveTypeId = (string)null,  // No leave type for overtime
                status = request.Status.ToString(),
                reason = request.Reason,
                messageOfDecider = request.Message,
                submitedDate = request.SubmitedDate,
                fromDate = request.RequestOverTime.DateOfOverTime, // Assuming DateOfOverTime is the start date
                toDate = (DateTime?)null, // No end date for overtime, can adjust if needed
                fromHour = request.RequestOverTime.FromHour.ToString("HH:mm"),
                toHour = request.RequestOverTime.ToHour.ToString("HH:mm"),
                actionDate = DateTime.Now,
                requestType = "Overtime",
                isSeen = false
            };

            var json = JsonSerializer.Serialize(firebaseData);
            var httpClient = new HttpClient();
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await httpClient.PutAsync($"https://nextjs-course-f2de1-default-rtdb.firebaseio.com{path}/{reverseTimestamp}.json", content);

            return result.IsSuccessStatusCode;
        }

        public async Task<dynamic> GetEmployeeOvertimeSummaryAndManagerName(Guid employeeId)
        {
            // Get the current date information
            var currentDate = DateTime.Now;
            var currentMonth = currentDate.Month;
            var currentYear = currentDate.Year;

            // Calculate the total overtime hours in the current month and year
            var totalOvertimeHoursInMonth = await _dbContext.Requests
                .Include(r => r.RequestOverTime)
                .Where(r => r.EmployeeSendRequestId == employeeId &&
                            r.Status == RequestStatus.Approved &&
                            r.requestType == RequestType.OverTime &&
                            r.RequestOverTime.DateOfOverTime.Year == currentYear &&
                            r.RequestOverTime.DateOfOverTime.Month == currentMonth)
                .SumAsync(r => r.RequestOverTime.NumberOfHour);

            var totalOvertimeHoursInYear = await _dbContext.Requests
                .Include(r => r.RequestOverTime)
                .Where(r => r.EmployeeSendRequestId == employeeId &&
                            r.Status == RequestStatus.Approved &&
                            r.requestType == RequestType.OverTime &&
                            r.RequestOverTime.DateOfOverTime.Year == currentYear)
                .SumAsync(r => r.RequestOverTime.NumberOfHour);

            // Get the manager's name
            var managerName = await _dbContext.Employees
                .Include(e => e.UserAccount)
                .Include(e => e.Department)
                .ThenInclude(d => d.Employees)
                .Where(e => e.Department.Employees.Any(emp => emp.Id == employeeId) && e.UserAccount.Role.Name == "Manager")
                .Select(e => e.FirstName + " " + e.LastName)
                .FirstOrDefaultAsync();

            return new
            {
                TotalOvertimeHoursInMonth = totalOvertimeHoursInMonth,
                TotalOvertimeHoursInYear = totalOvertimeHoursInYear,
                ManagerName = managerName
            };
        }

        public async Task<object> ApproveOvertimeRequestAndLogHours(Guid requestId, Guid? employeeIdDecider)
        {
            // Retrieve the Request by requestId
            var request = await _dbContext.Requests
                                          .Include(r => r.RequestOverTime)
                                          .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                throw new Exception("Request not found");
            }

            // Update the Request status to Approved
            request.Status = RequestStatus.Approved;
            request.EmployeeIdLastDecider = employeeIdDecider;

            // Assuming there's logic to log or handle the approved overtime hours in your system
            // For example, updating Workslot or related overtime management entity

            var overtimeDate = request.RequestOverTime.DateOfOverTime;

            // Update the AttendanceStatus or WorkingStatus if needed

            // Save changes to the database
            await _dbContext.SaveChangesAsync();

            await SendRequestOvertimeToEmployeeFirebase(requestId);

            return new { message = "Overtime request approved successfully" };
        }

        public async Task<object> RejectOvertimeRequest(RequestReasonDTO requestObj)
        {
            Guid requestId = requestObj.requestId;
            string reason = requestObj.reason;

            // Retrieve the request by requestId
            var request = await _dbContext.Requests
                                           .Include(r => r.RequestOverTime)
                                           .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                throw new Exception("RequestId not found.");
            }

            if (request.RequestOverTime == null)
            {
                throw new Exception("Request Overtime not found.");
            }

            // No need to check for past dates for overtime, but ensure it's not already processed
            if (request.Status == RequestStatus.Approved || request.Status == RequestStatus.Rejected)
            {
                throw new Exception("Overtime request cannot be rejected after being Approved or Rejected.");
            }

            // Set the Request status to Rejected based on your application logic
            request.Status = RequestStatus.Rejected;
            request.Message = reason;
            request.EmployeeIdLastDecider = requestObj.employeeIdDecider;

            // Save the changes to the database
            await _dbContext.SaveChangesAsync();
            await SendRequestOvertimeToEmployeeFirebase(requestId); // Assuming you have similar notification sending method

            return new { message = "Overtime request rejected successfully." };
        }

        public async Task<RequestOverTimeDTO> GetRequestOverTimeById(Guid requestId)
        {
            var request = await _dbContext.Requests
                .Include(r => r.RequestOverTime)
                    .ThenInclude(ro => ro.WorkingStatus)
                .Include(r => r.EmployeeSendRequest)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.requestType == RequestType.OverTime && !r.IsDeleted);

            if (request == null)
            {
                throw new Exception("Request not found or has been deleted.");
            }

            var requestOverTime = request.RequestOverTime;
            var employee = request.EmployeeSendRequest;

            // Assuming the request is approved; adjust logic if necessary
            var timeInMonth = _dbContext.Requests
                .Include(r => r.RequestOverTime)
                .Where(r => r.EmployeeSendRequestId == employee.Id && r.Status == RequestStatus.Approved &&
                            r.RequestOverTime.DateOfOverTime.Month == requestOverTime.DateOfOverTime.Month &&
                            r.RequestOverTime.DateOfOverTime.Year == requestOverTime.DateOfOverTime.Year)
                .Sum(r => r.RequestOverTime.NumberOfHour);

            var timeInYear = _dbContext.Requests
                .Include(r => r.RequestOverTime)
                .Where(r => r.EmployeeSendRequestId == employee.Id && r.Status == RequestStatus.Approved &&
                            r.RequestOverTime.DateOfOverTime.Year == requestOverTime.DateOfOverTime.Year)
                .Sum(r => r.RequestOverTime.NumberOfHour);

            var dto = new RequestOverTimeDTO
            {
                id = request.Id,
                employeeId = employee.Id,
                employeeName = $"{employee.FirstName} {employee.LastName}",
                employeeNumber = employee.EmployeeNumber,
                RequestOverTimeId = requestOverTime.Id,
                workingStatusId = requestOverTime.WorkingStatusId,
                timeInMonth = timeInMonth,
                timeInYear = timeInYear,
                workingStatus = requestOverTime.WorkingStatus.Name,
                Date = requestOverTime.DateOfOverTime.ToString("yyyy/MM/dd"),
                timeStart = requestOverTime.FromHour.ToString("HH:mm"),
                timeEnd = requestOverTime.ToHour.ToString("HH:mm"),
                NumberOfHour = requestOverTime.NumberOfHour,
                submitDate = request.SubmitedDate.ToString("yyyy/MM/dd"),
                IsDeleted = requestOverTime.IsDeleted,
                status = request.Status.ToString(),
                linkFile = request.PathAttachmentFile,
                reason = request.Reason,
                reasonReject = request.Message,
                deciderId = request.EmployeeIdLastDecider
            };

            return dto;
        }


    }
}
