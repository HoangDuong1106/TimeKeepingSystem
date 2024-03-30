using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { public interface IRequestRepository { Task<CombinedRequestDTO> GetAllRequestTypesOfEmployeeById(Guid employeeId); Task<bool> SoftDeleteAsync(Guid id); } }