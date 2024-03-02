using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { public interface IWorkDateSettingRepository { Task<object> Create(WorkDateSettingDTO dto); Task<bool> Delete(Guid id); Task<List<WorkDateSettingDTO>> GetAll(); Task<WorkDateSettingDTO> GetById(Guid id); Task<bool> SoftDeleteAsync(Guid id); Task<bool> Update(WorkDateSettingDTO dto); } }