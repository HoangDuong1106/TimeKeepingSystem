using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
        public async Task<CombinedRequestDTO> GetAllRequestTypesOfEmployeeById(Guid employeeId, string? dateFilter)
        {
            var combinedRequests = new CombinedRequestDTO();

            // Populate OverTimeRequests
            DateTime now = DateTime.Now;
            if (dateFilter != null)
            {
                now = DateTime.ParseExact(dateFilter, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            }
            var employeeOvertimeRequests = _dbContext.Requests
                .Include(r => r.RequestOverTime).ThenInclude(rot => rot.WorkingStatus)
                .Where(r => !r.IsDeleted && r.EmployeeSendRequestId == employeeId && r.requestType == RequestType.OverTime)
                .ToList(); // ToList to materialize the query if necessary for complex calculations

            var timeInYear = employeeOvertimeRequests
                .Where(r => r.RequestOverTime.DateOfOverTime.Year == now.Year)
                .Sum(r => r.RequestOverTime.NumberOfHour);

            var timeInMonth = employeeOvertimeRequests
                .Where(r => r.RequestOverTime.DateOfOverTime.Year == now.Year && r.RequestOverTime.DateOfOverTime.Month == now.Month)
                .Sum(r => r.RequestOverTime.NumberOfHour);

            if (dateFilter != null)
            {
                combinedRequests.OverTimeRequests = employeeOvertimeRequests
    .Where(r => r.RequestOverTime.DateOfOverTime.Year == now.Year && r.RequestOverTime.DateOfOverTime.Month == now.Month) // Filter by current month and year
    .Select(r => new RequestOverTimeDTO
    {
        id = r.Id,
        employeeId = r.EmployeeSendRequestId,
        employeeName = _dbContext.Employees.Where(e => e.Id == r.EmployeeSendRequestId).Select(e => e.FirstName + " " + e.LastName).FirstOrDefault(),
        employeeNumber = _dbContext.Employees.Where(e => e.Id == r.EmployeeSendRequestId).FirstOrDefault()?.EmployeeNumber,
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
            }
            else
            {
                combinedRequests.OverTimeRequests = employeeOvertimeRequests
    .Select(r => new RequestOverTimeDTO
    {
        id = r.Id,
        employeeId = r.EmployeeSendRequestId,
        employeeName = _dbContext.Employees.Where(e => e.Id == r.EmployeeSendRequestId).Select(e => e.FirstName + " " + e.LastName).FirstOrDefault(),
        employeeNumber = _dbContext.Employees.Where(e => e.Id == r.EmployeeSendRequestId).FirstOrDefault()?.EmployeeNumber,
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
            }

            // Populate WorkTimeRequests
            combinedRequests.WorkTimeRequests = await _dbContext.Requests
    .Include(r => r.RequestWorkTime).ThenInclude(rw => rw.WorkslotEmployee).ThenInclude(we => we.Workslot)
    .Where(r => !r.IsDeleted && r.EmployeeSendRequestId == employeeId && r.requestType == RequestType.WorkTime)
    .Select(r => new RequestWorkTimeDTO
    {
        Id = r.Id,
        employeeId = employeeId,
        employeeName = _dbContext.Employees.Where(e => e.Id == employeeId).Select(e => e.FirstName + " " + e.LastName).FirstOrDefault(),
        employeeNumber = _dbContext.Employees.Where(e => e.Id == employeeId).FirstOrDefault() != null ? _dbContext.Employees.Where(e => e.Id == employeeId).FirstOrDefault().EmployeeNumber : null,
        RealHourStart = r.RequestWorkTime.RealHourStart,
        RealHourEnd = r.RequestWorkTime.RealHourEnd,
        WorkslotEmployeeId = r.RequestWorkTime.WorkslotEmployeeId,
        DateOfWorkTime = r.RequestWorkTime.WorkslotEmployee.Workslot.DateOfSlot != null ? r.RequestWorkTime.WorkslotEmployee.Workslot.DateOfSlot.ToString("yyyy/MM/dd") : null,
        // Add other necessary fields from RequestWorkTimeDTO
    }).ToListAsync();

            // Populate LeaveRequests
            if (dateFilter != null)
            {
                combinedRequests.LeaveRequests = await _dbContext.Requests
    .Include(r => r.RequestLeave).ThenInclude(rl => rl.LeaveType)
    .Include(r => r.RequestLeave).ThenInclude(rl => rl.WorkslotEmployees).ThenInclude(we => we.Workslot)
    .Where(r => !r.IsDeleted && r.EmployeeSendRequestId == employeeId && r.requestType == RequestType.Leave
                && ((r.RequestLeave.FromDate.Year == now.Year && r.RequestLeave.FromDate.Month == now.Month)
                    || (r.RequestLeave.ToDate.Year == now.Year && r.RequestLeave.ToDate.Month == now.Month)))
    .Select(r => new LeaveRequestDTO
    {
        id = r.Id,
        employeeId = employeeId,
        employeeName = _dbContext.Employees.Where(e => e.Id == employeeId).Select(e => e.FirstName + " " + e.LastName).FirstOrDefault(),
        employeeNumber = _dbContext.Employees.Where(e => e.Id == employeeId).FirstOrDefault() != null ? _dbContext.Employees.Where(e => e.Id == employeeId).FirstOrDefault().EmployeeNumber : null,
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
            } else
            {
                combinedRequests.LeaveRequests = await _dbContext.Requests
                .Include(r => r.RequestLeave).ThenInclude(rl => rl.LeaveType)
                .Include(r => r.RequestLeave).ThenInclude(rl => rl.WorkslotEmployees).ThenInclude(we => we.Workslot)
                .Where(r => !r.IsDeleted && r.EmployeeSendRequestId == employeeId && r.requestType == RequestType.Leave)
                .Select(r => new LeaveRequestDTO
                {
                    id = r.Id,
                    employeeId = employeeId,
                    employeeName = _dbContext.Employees.Where(e => e.Id == employeeId).Select(e => e.FirstName + " " + e.LastName).FirstOrDefault(),
                    employeeNumber = _dbContext.Employees.Where(e => e.Id == employeeId).FirstOrDefault() != null ? _dbContext.Employees.Where(e => e.Id == employeeId).FirstOrDefault().EmploymentType : null,
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
            }

            return combinedRequests;
        }

    }
}
