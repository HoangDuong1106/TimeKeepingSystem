using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

            // Initialize new Request and RequestOvertime objects
            Request newRequest = new Request()
            {
                Id = Guid.NewGuid(),
                EmployeeSendRequestId = employeeId,
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

    }
}
