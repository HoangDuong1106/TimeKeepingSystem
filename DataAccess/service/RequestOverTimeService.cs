using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class RequestOverTimeService : IRequestOverTimeService
    {
        private readonly IRequestOverTimeRepository _requestOverTimeRepository;

        public RequestOverTimeService(IRequestOverTimeRepository requestOverTimeRepository)
        {
            _requestOverTimeRepository = requestOverTimeRepository;
        }

        // Implement the GetAllAsync method from IRequestOverTimeService by calling the corresponding repository method
        public async Task<List<RequestOverTimeDTO>> GetAllAsync()
        {
            return await _requestOverTimeRepository.GetAllAsync();
        }

        public async Task<object> CreateRequestOvertime(CreateRequestOverTimeDTO dto, Guid employeeId)
        {
            return await _requestOverTimeRepository.CreateRequestOvertime(dto, employeeId);
        }

        public object GetRequestOverTimeOfEmployeeById(Guid employeeId)
        {
            return  _requestOverTimeRepository.GetRequestOverTimeOfEmployeeById(employeeId);
        }

        public async Task<object> EditRequestOvertimeOfEmployee(EditRequestOverTimeDTO dto, Guid employeeId)
        {
            return await _requestOverTimeRepository.EditRequestOvertimeOfEmployee(dto, employeeId);
        }

        public List<RequestOverTimeDTO> GetAllRequestOverTime(string? nameSearch, int status, string month, Guid? employeeId)
        {
            var monthDate = DateTime.ParseExact(month, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            return _requestOverTimeRepository.GetAllRequestOverTime(nameSearch, status, monthDate, employeeId);
        }
    }
}