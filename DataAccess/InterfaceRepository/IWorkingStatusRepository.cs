using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { public interface IWorkingStatusRepository { Task<object> Create(WorkingStatusDTO newWorkingStatus); Task<bool> SoftDeleteAsync(Guid id); } }