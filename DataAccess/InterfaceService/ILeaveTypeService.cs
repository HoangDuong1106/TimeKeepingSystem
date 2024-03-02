using BusinessObject.DTO;
using System;

namespace DataAccess.InterfaceService
{
    public interface ILeaveTypeService
    {
        // Empty interface
        Task<List<LeaveTypeDTO>> GetAllAsync();
    }
}
