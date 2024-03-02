using BusinessObject.DTO;
using System;

namespace DataAccess.InterfaceService
{
    public interface IWorkDateSettingService
    {
        Task<object> Create(WorkDateSettingDTO dto);
        Task<bool> Delete(Guid id);

        // Empty interface
        Task<List<WorkDateSettingDTO>> GetAll();
        Task<WorkDateSettingDTO> GetById(Guid id);
        Task<bool> Update(WorkDateSettingDTO dto);
    }
}
