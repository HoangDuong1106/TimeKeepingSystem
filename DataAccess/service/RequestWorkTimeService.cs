using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;  // Assuming the repository interfaces are in this namespace
using DataAccess.Service;  // Assuming the service interfaces are in this namespace

namespace DataAccess.Service
{
    public class RequestWorkTimeService : IRequestWorkTimeService
    {
        private readonly IRequestWorkTimeRepository _requestWorkTimeRepository;

        public RequestWorkTimeService(IRequestWorkTimeRepository requestWorkTimeRepository)
        {
            _requestWorkTimeRepository = requestWorkTimeRepository;
        }

        // Implement the GetAllAsync method from IRequestWorkTimeService by calling the corresponding repository method
        public object GetRequestWorkTimeOfEmployeeById(Guid employeeId)
        {
            return _requestWorkTimeRepository.GetRequestWorkTimeOfEmployeeById(employeeId);
        }

        public async Task<object> CreateRequestWorkTime(RequestWorkTimeDTO dto, Guid employeeId)
        {
            return await _requestWorkTimeRepository.CreateRequestWorkTime(dto, employeeId);
        }

        public async Task<object> EditRequestWorkTime(RequestWorkTimeDTO dto)
        {
            return await _requestWorkTimeRepository.EditRequestWorkTime(dto);
        }

        public List<WorkslotEmployeeDTO> GetWorkslotEmployeesWithLessThanNineHours(Guid employeeId)
        {
            return _requestWorkTimeRepository.GetWorkslotEmployeesWithLessThanNineHours(employeeId);
        }

        public List<RequestWorkTimeDTO> GetAllRequestWorkTime(string? nameSearch, int status, string? month)
        {
            return _requestWorkTimeRepository.GetAllRequestWorkTime(nameSearch, status, month);
        }

        public async Task<object> ApproveRequestWorkTime(Guid requestId) => await _requestWorkTimeRepository.ApproveRequestWorkTime(requestId);
    }
}