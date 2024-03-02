using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class RiskPerformanceSettingService : IRiskPerformanceSettingService
    {
        private readonly IRiskPerformanceSettingRepository _riskPerformanceSettingRepository;

        public RiskPerformanceSettingService(IRiskPerformanceSettingRepository riskPerformanceSettingRepository)
        {
            _riskPerformanceSettingRepository = riskPerformanceSettingRepository;
        }
    }
}