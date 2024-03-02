using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class AttendanceStatusRepository : Repository<AttendanceStatus>, IAttendanceStatusRepository
    {
        private readonly MyDbContext _dbContext;

        public AttendanceStatusRepository(MyDbContext context) : base(context)
        {
            // You can add more specific methods here if needed
            _dbContext = context;
        }

        public async Task<List<AttendanceStatusDTO>> GetAllAsync()
        {
            var ass = await base.GetAllAsync();
            return await ass.Include(a => a.WorkingStatus).Include(a => a.LeaveType).Select(a => new AttendanceStatusDTO
            {
                Id = a.Id,
                LeaveTypeId = a.LeaveTypeId,
                WorkingStatusId = a.WorkingStatusId,
                Name = a.LeaveTypeId != null ? a.LeaveType.Name : a.WorkingStatus.Name,
                IsDeleted = a.IsDeleted
            }).ToListAsync();
        }

        public async Task<bool> AddAsync(AttendanceStatusDTO dto)
        {
            try
            {
                _dbContext.AttendanceStatuses.Add(new AttendanceStatus() // have dbSaveChange inside method
                {
                    Id = Guid.NewGuid(),
                    LeaveTypeId = dto.LeaveTypeId,
                    WorkingStatusId = dto.WorkingStatusId,
                    IsDeleted = false
                    //LeaveType = _dbContext.LeaveTypes.FirstOrDefault(l => l.Id == dto.LeaveTypeId),
                    //WorkingStatus =  _dbContext.WorkingStatuses.FirstOrDefault(w => w.Id == dto.WorkingStatusId)
                });

                await _dbContext.SaveChangesAsync();
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
    }
}
