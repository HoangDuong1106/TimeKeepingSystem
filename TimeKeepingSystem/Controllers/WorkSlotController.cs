
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using Microsoft.AspNetCore.Mvc;

namespace TimeKeepingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkSlotController : ControllerBase
    {
        private readonly IWorkslotService _service;
        private readonly IWorkslotRepository _repository;

        public WorkSlotController(IWorkslotService service, IWorkslotRepository repository)
        {
            _service = service;
            _repository = repository;
        }

        // GET: api/AttendanceStatus
        [HttpPost("generate-workslot-of-department-in-one-month")]
        public async Task<List<Workslot>> GenerateWorkSlotsForMonth(CreateWorkSlotRequest request)
        {
            return await _service.GenerateWorkSlotsForMonth(request);
        }

        [HttpGet("get-workslot-of-department-in-one-month")]
        public async Task<List<object>> GetWorkSlotsForDepartment(Guid departmentId, string month)
        {
            return await _repository.GetWorkSlotsForDepartment(new CreateWorkSlotRequest() { departmentId = departmentId, month = month});
        }
    }
}

