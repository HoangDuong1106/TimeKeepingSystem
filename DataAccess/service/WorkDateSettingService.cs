using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class WorkDateSettingService : IWorkDateSettingService
    {
        private readonly IWorkDateSettingRepository _workDateSettingRepository;

        public WorkDateSettingService(IWorkDateSettingRepository workDateSettingRepository)
        {
            _workDateSettingRepository = workDateSettingRepository;
        }

        public async Task<List<WorkDateSettingDTO>> GetAll()
        {
            return await _workDateSettingRepository.GetAll();
        }

        public async Task<object> Create(WorkDateSettingDTO dto)
        {
            return await _workDateSettingRepository.Create(dto);
        }

        public async Task<WorkDateSettingDTO> GetById(Guid id)
        {
            return await _workDateSettingRepository.GetById(id);
        }

        public async Task<bool> Update(WorkDateSettingDTO dto)
        {
            return await _workDateSettingRepository.Update(dto);
        }

        public async Task<bool> Delete(Guid id)
        {
            return await _workDateSettingRepository.Delete(id);
        }
    }
}