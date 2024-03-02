using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class WorkTrackSettingService : IWorkTrackSettingService
    {
        private readonly IWorkTrackSettingRepository _WorkTrackSettingRepository;

        public WorkTrackSettingService(IWorkTrackSettingRepository WorkTrackSettingRepository)
        {
            _WorkTrackSettingRepository = WorkTrackSettingRepository;
        }

        // Implement the GetAllAsync method from IWorkTrackSettingService by calling the corresponding repository method
    }
}