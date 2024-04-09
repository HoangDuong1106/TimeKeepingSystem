using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using System.Text;

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
            var result = new List<object>();
            var list = _dbContext.Requests.Include(r => r.RequestOverTime).Where(r => r.IsDeleted == false)
                 .Where(r => r.EmployeeSendRequestId == employeeId && r.requestType == RequestType.OverTime)
                 .ToList();
            list.ForEach(r =>
            {
                result.Add(new RequestOverTimeDTO()
                {
                    id = r.Id,
                    RequestOverTimeId = r.RequestOverTimeId,
                    Name = r.RequestOverTime?.Name ?? "",
                    Date = r.RequestOverTime.DateOfOverTime.ToString("yyyy/MM/dd"),
                    timeStart = r.RequestOverTime.FromHour.ToString("HH:mm"),
                    NumberOfHour = r.RequestOverTime.NumberOfHour,
                    timeEnd = r.RequestOverTime.ToHour.ToString("HH:mm"),
                    statusRequest = r.Status,
                    status = r.Status.ToString(),
                    reason = r.Reason,
                    linkFile = r.PathAttachmentFile,
                    workingStatus = _dbContext.WorkingStatuses.FirstOrDefault(ws => ws.Id == r.RequestOverTime.WorkingStatusId)?.Name ?? "",
                    workingStatusId = r.RequestOverTime.WorkingStatusId,
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

            Employee employeeSendRequest = _dbContext.Employees.FirstOrDefault(e => e.Id == employeeId);
            if (employeeSendRequest == null)
            {
                throw new Exception("Employee Send Request Not Found");
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
                requestType = RequestType.OverTime
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

        public async Task<object> EditRequestOvertimeOfEmployee(EditRequestOverTimeDTO dto, Guid employeeId)
        {
            // Step 1: Retrieve the record from the database using its ID
            Request request = _dbContext.Requests.Include(r => r.RequestOverTime).Where(r => r.IsDeleted == false).Where(r => r.Id == dto.requestId && r.EmployeeSendRequestId == employeeId).FirstOrDefault();
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

        public List<RequestOverTimeDTO> GetAllRequestOverTime(string? nameSearch, int status, DateTime month)
        {
            var result = new List<RequestOverTimeDTO>();
            var list = _dbContext.Requests
                .Include(r => r.RequestOverTime)
                .ThenInclude(ro => ro.WorkingStatus)
                .Where(r => r.IsDeleted == false)
                .Where(r => r.requestType == RequestType.OverTime);

            if (status != -1) list = list.Where(r => (int)r.Status == status);

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
                    reason = r.Reason
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
            request.Reason = reason;
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

            var firebaseData = new
            {
                requestId = request.Id,
                employeeSenderId = request.EmployeeSendRequestId,
                employeeSenderName = request.EmployeeSendRequest != null ? request.EmployeeSendRequest.FirstName + " " + request.EmployeeSendRequest.LastName : null,
                employeeDeciderId = request.EmployeeIdLastDecider,
                employeeDeciderName = manager != null ? manager.FirstName + " " + manager.LastName : null,
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
            var result = await httpClient.PostAsync($"https://nextjs-course-f2de1-default-rtdb.firebaseio.com/{path}.json", content);

            return result.IsSuccessStatusCode;
        }



    }
}
