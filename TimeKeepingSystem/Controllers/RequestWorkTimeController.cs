
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using Microsoft.AspNetCore.Mvc;

namespace TimeKeepingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestWorkTimeController : ControllerBase
    {
        private readonly IRequestWorkTimeService _service;
        private readonly IRequestWorkTimeRepository _repository;

        public RequestWorkTimeController(IRequestWorkTimeService service, IRequestWorkTimeRepository repository)
        {
            _service = service;
            _repository = repository;
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
                return Ok(_repository.GetRequestWorkTimeOfEmployeeById(employeeId));
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
        public ActionResult<List<WorkslotEmployeeDTO>> GetWorkslotEmployeesWithLessThanNineHours(Guid employeeId, bool? isWorkLate = false, bool? isLeaveSoon = false, bool? isNotCheckIn = false, bool? isNotCheckOut = false)
        {
            try
            {
                return Ok(_repository.GetWorkslotEmployeesWithLessThanNineHours(employeeId, isWorkLate, isLeaveSoon, isNotCheckIn, isNotCheckOut));
            } catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-all-work-time-request")]
        public ActionResult<List<RequestWorkTimeDTO>> GetAllRequestWorkTime(string? nameSearch, int? status, string? month, Guid? employeeId)
        {
            try
            {
                return Ok(_repository.GetAllRequestWorkTime(nameSearch, status, month, employeeId));
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

        [HttpPatch("reject-work-time-request")]
        public async Task<object> RejectWorkTimeRequest(RequestReasonDTO requestObj)
        {
            try
            {
                return Ok(await _repository.RejectWorkTimeRequest(requestObj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("cancel-approved-work-time-request")]
        public async Task<object> CancelApprovedWorkTimeRequest(RequestReasonDTO requestObj)
        {
            try
            {
                return Ok(await _repository.CancelApprovedWorkTimeRequest(requestObj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}

