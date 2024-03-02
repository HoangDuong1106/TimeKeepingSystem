using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class RiskPerformanceEmployeeService : IRiskPerformanceEmployeeService
    {
        private readonly IRiskPerformanceEmployeeRepository _riskPerformanceEmployeeRepository;

        public RiskPerformanceEmployeeService(IRiskPerformanceEmployeeRepository riskPerformanceEmployeeRepository)
        {
            _riskPerformanceEmployeeRepository = riskPerformanceEmployeeRepository;
        }

    }
}