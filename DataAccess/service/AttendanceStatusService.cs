using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;

namespace DataAccess.Service
{
    public class AttendanceStatusService : IAttendanceStatusService
    {
        private readonly IAttendanceStatusRepository _attendancestatusrepository;

        public AttendanceStatusService(IAttendanceStatusRepository attendancestatusrepository)
        {
            _attendancestatusrepository = attendancestatusrepository;
            
        }

        public async Task<List<AttendanceStatusDTO>> GetAllAsync() => await _attendancestatusrepository.GetAllAsync();
        public async Task<bool> AddAsync(AttendanceStatusDTO dto) => await _attendancestatusrepository.AddAsync(dto);
        public async Task<bool> SoftDeleteAsync(Guid id) => await _attendancestatusrepository.SoftDeleteAsync(id);

    }
}
