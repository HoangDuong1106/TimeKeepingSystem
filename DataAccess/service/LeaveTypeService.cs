using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class LeaveTypeService : ILeaveTypeService
    {
        private readonly ILeaveTypeRepository _leaveTypeRepository;

        public LeaveTypeService(ILeaveTypeRepository leaveTypeRepository)
        {
            _leaveTypeRepository = leaveTypeRepository;
        }

        // Implement the GetAllAsync method from ILeaveTypeService by calling the corresponding repository method
        public async Task<List<LeaveTypeDTO>> GetAllAsync()
        {
            return await _leaveTypeRepository.GetAllAsync();
        }
    }
}