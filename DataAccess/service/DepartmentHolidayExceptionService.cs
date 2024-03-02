using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class DepartmentHolidayExceptionService : IDepartmentHolidayExceptionService
    {
        private readonly IDepartmentHolidayExceptionRepository _DepartmentHolidayExceptionRepository;

        public DepartmentHolidayExceptionService(IDepartmentHolidayExceptionRepository DepartmentHolidayExceptionRepository)
        {
            _DepartmentHolidayExceptionRepository = DepartmentHolidayExceptionRepository;
        }

        // Implement the GetAllAsync method from IDepartmentHolidayExceptionService by calling the corresponding repository method
        public async Task<List<DepartmentHolidayExceptionDTO>> GetAllAsync()
        {
            return await _DepartmentHolidayExceptionRepository.GetAllAsync();
        }
    }
}