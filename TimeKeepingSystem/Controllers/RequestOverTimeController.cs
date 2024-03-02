
using BusinessObject.DTO;
using DataAccess.InterfaceService;
using Microsoft.AspNetCore.Mvc;

namespace TimeKeepingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestOverTimeController : ControllerBase
    {
        private readonly IAttendanceStatusService _service;
        private readonly IRequestLeaveService _requestLeaveService;
        private readonly IRequestOverTimeService _requestOverTimeService;

        public RequestOverTimeController(IAttendanceStatusService service, IRequestLeaveService requestLeaveService, IRequestOverTimeService requestOverTimeService)
        {
            _service = service;
            _requestLeaveService = requestLeaveService;
            _requestOverTimeService = requestOverTimeService;
        }

        [HttpPost("create-request-over-time-of-employee")]
        public async Task<object> CreateRequestOvertime(CreateRequestOverTimeDTO dto, Guid employeeId)
        {
            try
            {
                return Ok(await _requestOverTimeService.CreateRequestOvertime(dto, employeeId));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-request-over-time-of-employee")]
        public ActionResult<object> GetRequestOverTimeOfEmployeeById(Guid employeeId)
        {
            try
            {
                return Ok(_requestOverTimeService.GetRequestOverTimeOfEmployeeById(employeeId));
            } catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("edit-request-over-time-of-employee")]
        public async Task<ActionResult<object>> EditRequestOvertimeOfEmployee([FromBody]EditRequestOverTimeDTO dto, Guid employeeId)
        {
            try
            {
                return Ok(await _requestOverTimeService.EditRequestOvertimeOfEmployee(dto, employeeId));
            } catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-all-request-over-time")]
        public ActionResult<List<RequestOverTimeDTO>> GetAllRequestOverTime(string? nameSearch, int status, string month)
        {
            try
            {
                return Ok(_requestOverTimeService.GetAllRequestOverTime(nameSearch, status, month));
            } catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

