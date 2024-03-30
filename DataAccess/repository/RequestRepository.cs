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
            var overtimeRequests = _dbContext.Requests.Include(r => r.RequestOverTime)
                .Where(r => !r.IsDeleted && r.EmployeeSendRequestId == employeeId && r.requestType == RequestType.OverTime)
                .Select(r => new RequestOverTimeDTO
                {
                    // Assuming the mapping is correct; adjust based on your actual RequestOverTimeDTO structure
                    id = r.Id,
                    RequestOverTimeId = r.RequestOverTimeId,
                    Name = r.RequestOverTime.Name,
                    Date = r.RequestOverTime.DateOfOverTime.ToString("yyyy/MM/dd"),
                    timeStart = r.RequestOverTime.FromHour.ToString("HH:mm"),
                    timeEnd = r.RequestOverTime.ToHour.ToString("HH:mm"),
                    // Continue mapping other fields...
                }).ToListAsync();

            combinedRequests.OverTimeRequests = await overtimeRequests;

            // Populate WorkTimeRequests
            var workTimeRequests = _dbContext.Requests.Include(r => r.RequestWorkTime).ThenInclude(rl => rl.WorkslotEmployee).ThenInclude(we => we.Workslot)
                .Where(r => !r.IsDeleted && r.EmployeeSendRequestId == employeeId && r.requestType == RequestType.WorkTime)
                .Select(r => new RequestWorkTimeDTO
                {
                    // Adjust based on your actual RequestWorkTimeDTO structure
                    Id = r.Id,
                    Name = r.RequestWorkTime.Name,
                    // Continue mapping other fields...
                }).ToListAsync();

            combinedRequests.WorkTimeRequests = await workTimeRequests;

            // Populate LeaveRequests
            var leaveRequests = _dbContext.Requests.Include(r => r.RequestLeave).ThenInclude(rl => rl.LeaveType).Include(r => r.RequestLeave).ThenInclude(rl => rl.WorkslotEmployees).ThenInclude(we => we.Workslot)
                .Where(r => !r.IsDeleted && r.EmployeeSendRequestId == employeeId && r.requestType == RequestType.Leave)
                .Select(r => new LeaveRequestDTO
                {
                    // Adjust based on your actual LeaveRequestDTO structure
                    id = r.Id,
                    employeeName = _dbContext.Employees.Where(e => e.Id == employeeId).Select(e => e.FirstName + " " + e.LastName).FirstOrDefault(),
                    // Continue mapping other fields...
                }).ToListAsync();

            combinedRequests.LeaveRequests = await leaveRequests;

            return combinedRequests;
        }

    }
}
