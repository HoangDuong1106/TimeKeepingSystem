using BusinessObject.DTO;
using System;

namespace DataAccess.InterfaceService
{
    public interface IWorkslotEmployeeService
    {
        Task<object> CheckInWorkslotEmployee(Guid employeeId, DateTime? currentTime);
        Task<string> ExportWorkSlotEmployeeReport(Guid departmentId);

        // Empty interface
        Task<object> GenerateWorkSlotEmployee(CreateWorkSlotRequest request);
        Task<object> GetWorkSlotEmployeeByEmployeeId(Guid employeeId);
    }
}
