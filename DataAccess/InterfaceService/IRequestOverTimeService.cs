using BusinessObject.DTO;
using System;

namespace DataAccess.InterfaceService
{
    public interface IRequestOverTimeService
    {
        Task<object> CreateRequestOvertime(CreateRequestOverTimeDTO dto, Guid employeeId);
        Task<object> EditRequestOvertimeOfEmployee(EditRequestOverTimeDTO dto, Guid employeeId);

        // Empty interface
        Task<List<RequestOverTimeDTO>> GetAllAsync();
        List<RequestOverTimeDTO> GetAllRequestOverTime(string? nameSearch, int status, string month);
        object GetRequestOverTimeOfEmployeeById(Guid employeeId);
    }
}
