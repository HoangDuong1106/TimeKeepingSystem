using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class RequestRepository : Repository<Request>, IRequestRepository
    {
        private readonly MyDbContext _dbContext;

        public RequestRepository(MyDbContext context) : base(context)
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
        public async Task<CombinedRequestDTO> GetAllRequestTypesOfEmployeeById(Guid employeeId)
        {
            var combinedRequests = new CombinedRequestDTO();

            // Populate OverTimeRequests
            DateTime now = DateTime.Now;
            var employeeOvertimeRequests = _dbContext.Requests
                .Include(r => r.RequestOverTime).ThenInclude(rot => rot.WorkingStatus)
                .Where(r => !r.IsDeleted && r.EmployeeSendRequestId == employeeId && r.requestType == RequestType.OverTime && r.Status == RequestStatus.Approved)
                .ToList(); // ToList to materialize the query if necessary for complex calculations

            var timeInYear = employeeOvertimeRequests
                .Where(r => r.RequestOverTime.DateOfOverTime.Year == now.Year)
                .Sum(r => r.RequestOverTime.NumberOfHour);

            var timeInMonth = employeeOvertimeRequests
                .Where(r => r.RequestOverTime.DateOfOverTime.Year == now.Year && r.RequestOverTime.DateOfOverTime.Month == now.Month)
                .Sum(r => r.RequestOverTime.NumberOfHour);

            combinedRequests.OverTimeRequests = employeeOvertimeRequests
    .Select(r => new RequestOverTimeDTO
    {
        id = r.Id,
        employeeId = r.EmployeeSendRequestId,
        employeeName = _dbContext.Employees.Where(e => e.Id == r.EmployeeSendRequestId).Select(e => e.FirstName + " " + e.LastName).FirstOrDefault(),
        RequestOverTimeId = r.RequestOverTimeId,
        workingStatusId = r.RequestOverTime.WorkingStatusId,
        timeStart = r.RequestOverTime.FromHour.ToString("HH:mm"),
        workingStatus = r.RequestOverTime.WorkingStatus.Name,
        timeEnd = r.RequestOverTime.ToHour.ToString("HH:mm"),
        Date = r.RequestOverTime.DateOfOverTime.ToString("yyyy/MM/dd"),
        NumberOfHour = r.RequestOverTime.NumberOfHour,
        submitDate = r.SubmitedDate.ToString("yyyy/MM/dd"),
        status = r.Status.ToString(),
        IsDeleted = r.RequestOverTime.IsDeleted,
        linkFile = r.PathAttachmentFile ?? "",
        reason = r.Reason,
        timeInMonth = timeInMonth,  // pre-calculated value
        timeInYear = timeInYear  // pre-calculated value
    }).ToList();

            // Populate WorkTimeRequests
            combinedRequests.WorkTimeRequests = await _dbContext.Requests
    .Include(r => r.RequestWorkTime).ThenInclude(rw => rw.WorkslotEmployee).ThenInclude(we => we.Workslot)
    .Where(r => !r.IsDeleted && r.EmployeeSendRequestId == employeeId && r.requestType == RequestType.WorkTime)
    .Select(r => new RequestWorkTimeDTO
    {
        Id = r.Id,
        employeeId = employeeId,
        employeeName = _dbContext.Employees.Where(e => e.Id == employeeId).Select(e => e.FirstName + " " + e.LastName).FirstOrDefault(),
        RealHourStart = r.RequestWorkTime.RealHourStart,
        RealHourEnd = r.RequestWorkTime.RealHourEnd,
        WorkslotEmployeeId = r.RequestWorkTime.WorkslotEmployeeId,
        DateOfWorkTime = r.RequestWorkTime.WorkslotEmployee.Workslot.DateOfSlot != null ? r.RequestWorkTime.WorkslotEmployee.Workslot.DateOfSlot.ToString("yyyy/MM/dd") : null,
        // Add other necessary fields from RequestWorkTimeDTO
    }).ToListAsync();

            // Populate LeaveRequests
            combinedRequests.LeaveRequests = await _dbContext.Requests
                .Include(r => r.RequestLeave).ThenInclude(rl => rl.LeaveType)
                .Include(r => r.RequestLeave).ThenInclude(rl => rl.WorkslotEmployees).ThenInclude(we => we.Workslot)
                .Where(r => !r.IsDeleted && r.EmployeeSendRequestId == employeeId && r.requestType == RequestType.Leave)
                .Select(r => new LeaveRequestDTO
                {
                    id = r.Id,
                    employeeId = employeeId,
                    employeeName = _dbContext.Employees.Where(e => e.Id == employeeId).Select(e => e.FirstName + " " + e.LastName).FirstOrDefault(),
                    submitDate = r.SubmitedDate.ToString("yyyy/MM/dd"),
                    startDate = r.RequestLeave.FromDate.ToString("yyyy/MM/dd"),
                    endDate = r.RequestLeave.ToDate.ToString("yyyy/MM/dd"),
                    leaveTypeId = r.RequestLeave.LeaveTypeId,
                    leaveType = r.RequestLeave.LeaveType.Name,
                    status = (int)r.Status,
                    statusName = r.Status.ToString(),
                    reason = r.Reason,
                    linkFile = r.PathAttachmentFile,
                    numberOfLeaveDate = r.RequestLeave.WorkslotEmployees.Count
                    // Add other necessary fields from LeaveRequestDTO
                }).ToListAsync();
                    
            return combinedRequests;
        }

    }
}
