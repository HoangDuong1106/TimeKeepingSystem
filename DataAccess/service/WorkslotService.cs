using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class WorkslotService : IWorkslotService
    {
        private readonly IWorkslotRepository _workslotRepository;

        public WorkslotService(IWorkslotRepository workslotRepository)
        {
            _workslotRepository = workslotRepository;
        }

        public async Task<List<Workslot>> GenerateWorkSlotsForMonth(CreateWorkSlotRequest request)
        {
            return await _workslotRepository.GenerateWorkSlotsForMonth(request);
        }

        public async Task<List<object>> GetWorkSlotsForDepartment(CreateWorkSlotRequest request)
        {
            return await _workslotRepository.GetWorkSlotsForDepartment(request);
        }
    }
}