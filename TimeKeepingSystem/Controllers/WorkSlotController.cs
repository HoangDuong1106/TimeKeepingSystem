
using BusinessObject.DTO;
using DataAccess.InterfaceService;
using Microsoft.AspNetCore.Mvc;

namespace TimeKeepingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkSlotController : ControllerBase
    {
        private readonly IWorkslotService _service;

        public WorkSlotController(IWorkslotService service)
        {
            _service = service;
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
            return await _service.GetWorkSlotsForDepartment(new CreateWorkSlotRequest() { departmentId = departmentId, month = month});
        }
    }
}

