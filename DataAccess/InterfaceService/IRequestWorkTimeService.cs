using BusinessObject.DTO;
using System;

namespace DataAccess.InterfaceService
{
    public interface IRequestWorkTimeService
    {
        Task<object> ApproveRequestWorkTime(Guid requestId);
        Task<object> CreateRequestWorkTime(RequestWorkTimeDTO dto, Guid employeeId);
        Task<object> EditRequestWorkTime(RequestWorkTimeDTO dto);
        List<RequestWorkTimeDTO> GetAllRequestWorkTime(string? nameSearch, int status, string? month);

        // Empty interface
        object GetRequestWorkTimeOfEmployeeById(Guid employeeId);
        List<WorkslotEmployeeDTO> GetWorkslotEmployeesWithLessThanNineHours(Guid employeeId);
    }
}
