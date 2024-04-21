using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { public interface IRequestRepository { Task<List<Guid>> FindRequestsWithMissingEmployees(); Task<CombinedRequestDTO> GetAllRequestTypesOfEmployeeById(Guid employeeId, string? dateFilter); Task<bool> SoftDeleteAsync(Guid id); Task<int> SoftDeleteInvalidRequests(); } }