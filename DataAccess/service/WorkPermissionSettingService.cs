using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class WorkPermissionSettingService : IWorkPermissionSettingService
    {
        private readonly IWorkPermissionSettingRepository _workPermissionSettingRepository;

        public WorkPermissionSettingService(IWorkPermissionSettingRepository workPermissionSettingRepository)
        {
            _workPermissionSettingRepository = workPermissionSettingRepository;
        }

        // Implement the GetAllAsync method from IWorkPermissionSettingService by calling the corresponding repository method

    }
}