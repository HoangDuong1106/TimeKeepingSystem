using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class WorkingStatusService : IWorkingStatusService
    {
        private readonly IWorkingStatusRepository _workingStatusRepository;

        public WorkingStatusService(IWorkingStatusRepository workingStatusRepository)
        {
            _workingStatusRepository = workingStatusRepository;
        }

        // Implement the GetAllAsync method from IWorkingStatusService by calling the corresponding repository method

    }
}