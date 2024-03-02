using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository
{
    public interface IAttendanceStatusRepository
    {
        Task<bool> AddAsync(AttendanceStatusDTO dto);
        Task<List<AttendanceStatusDTO>> GetAllAsync();
        Task<bool> SoftDeleteAsync(Guid id);
    }
}
