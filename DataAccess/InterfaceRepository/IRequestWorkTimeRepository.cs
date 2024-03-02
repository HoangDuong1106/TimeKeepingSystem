using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { public interface IRequestWorkTimeRepository { Task<object> ApproveRequestWorkTime(Guid requestId); Task<object> CreateRequestWorkTime(RequestWorkTimeDTO dto, Guid employeeId); Task<object> EditRequestWorkTime(RequestWorkTimeDTO dto); List<RequestWorkTimeDTO> GetAllRequestWorkTime(string? nameSearch, int status, string? month); object GetRequestWorkTimeOfEmployeeById(Guid employeeId); List<WorkslotEmployeeDTO> GetWorkslotEmployeesWithLessThanNineHours(Guid employeeId); Task<bool> SoftDeleteAsync(Guid id); } }