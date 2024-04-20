using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { public interface IRequestRepository { Task<CombinedRequestDTO> GetAllRequestTypesOfEmployeeById(Guid employeeId, string? dateFilter); Task<bool> SoftDeleteAsync(Guid id); } }