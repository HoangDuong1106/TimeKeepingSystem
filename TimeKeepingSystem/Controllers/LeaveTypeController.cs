
using BusinessObject.DTO;
using DataAccess.InterfaceService;
using Microsoft.AspNetCore.Mvc;

namespace TimeKeepingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveTypeController : ControllerBase
    {
        private readonly IAttendanceStatusService _service;
        private readonly IRequestLeaveService _requestLeaveService;
        private readonly ILeaveTypeService _leaveTypeService;

        public LeaveTypeController(IAttendanceStatusService service, IRequestLeaveService requestLeaveService, ILeaveTypeService leaveTypeService)
        {
            _service = service;
            _requestLeaveService = requestLeaveService;
            _leaveTypeService = leaveTypeService;
        }

        [HttpGet("get-all-leave-type")]
        public async Task<ActionResult<List<LeaveTypeDTO>>> GetAllAsync()
        {
            try
            {
                return Ok(await _leaveTypeService.GetAllAsync());
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}

