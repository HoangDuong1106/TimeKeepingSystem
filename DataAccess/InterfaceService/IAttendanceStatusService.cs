using BusinessObject.DTO;
using System;

namespace DataAccess.InterfaceService
{
    public interface IAttendanceStatusService
    {
        Task<bool> AddAsync(AttendanceStatusDTO dto);

        // Empty interface
        Task<List<AttendanceStatusDTO>> GetAllAsync();
        Task<bool> SoftDeleteAsync(Guid id);
    }
}
