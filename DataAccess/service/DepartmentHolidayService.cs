using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class DepartmentHolidayService : IDepartmentHolidayService
    {
        private readonly IHolidayRepository _DepartmentHolidayRepository;

        public DepartmentHolidayService(IHolidayRepository DepartmentHolidayRepository)
        {
            _DepartmentHolidayRepository = DepartmentHolidayRepository;
        }

        // Implement the GetAllAsync method from IDepartmentHolidayService by calling the corresponding repository method
        public async Task<List<DepartmentHolidayDTO>> GetAllAsync()
        {
            return await _DepartmentHolidayRepository.GetAllAsync();
        }
    }
}