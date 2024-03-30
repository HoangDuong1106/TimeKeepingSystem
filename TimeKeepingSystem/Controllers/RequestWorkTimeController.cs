
using BusinessObject.DTO;
using DataAccess.InterfaceService;
using Microsoft.AspNetCore.Mvc;

namespace TimeKeepingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestWorkTimeController : ControllerBase
    {
        private readonly IRequestWorkTimeService _service;

        public RequestWorkTimeController(IRequestWorkTimeService service)
        {
            _service = service;
        }

        [HttpPost("create-request-work-time-of-employee")]
        public async Task<ActionResult<object>> CreateRequestWorkTime(RequestWorkTimeDTO dto, Guid employeeId)
        {
            try
            {
                return Ok(await _service.CreateRequestWorkTime(dto, employeeId));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-request-work-time-of-employee")]
        public ActionResult<object> GetRequestWorkTimeOfEmployeeById(Guid employeeId)
        {
            try
            {
                return Ok(_service.GetRequestWorkTimeOfEmployeeById(employeeId));
            } catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("edit-request-work-time")]
        public async Task<ActionResult<object>> EditRequestWorkTime(RequestWorkTimeDTO dto)
        {
            try {
                return Ok(await _service.EditRequestWorkTime(dto));
            } catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-workslot-lack-time-of-employee")]
        public ActionResult<List<WorkslotEmployeeDTO>> GetWorkslotEmployeesWithLessThanNineHours(Guid employeeId)
        {
            try
            {
                return Ok(_service.GetWorkslotEmployeesWithLessThanNineHours(employeeId));
            } catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-all-work-time-request")]
        public ActionResult<List<RequestWorkTimeDTO>> GetAllRequestWorkTime(string? nameSearch, int? status, string? month)
        {
            try
            {
                return Ok(_service.GetAllRequestWorkTime(nameSearch, status, month));
            } catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("approve-work-time-request")]
        public async Task<IActionResult> ApproveRequestWorkTime(Guid requestId)
        {
            try
            {
                return Ok(await _service.ApproveRequestWorkTime(requestId));
            } catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}

