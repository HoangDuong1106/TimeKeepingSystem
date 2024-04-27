
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
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
        private readonly IRequestOverTimeRepository _requestOverTimeRepository;

        public RequestOverTimeController(IAttendanceStatusService service, IRequestLeaveService requestLeaveService, IRequestOverTimeService requestOverTimeService, IRequestOverTimeRepository requestOverTimeRepository)
        {
            _service = service;
            _requestLeaveService = requestLeaveService;
            _requestOverTimeService = requestOverTimeService;
            _requestOverTimeRepository = requestOverTimeRepository;
        }

        [HttpPost("create-request-over-time-of-employee")]
        public async Task<object> CreateRequestOvertime(CreateRequestOverTimeDTO dto, Guid employeeId)
        {
            try
            {
                return Ok(await _requestOverTimeRepository.CreateRequestOvertime(dto, employeeId));
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
        public ActionResult<List<RequestOverTimeDTO>> GetAllRequestOverTime(string? nameSearch, int status, string month, Guid? employeeId)
        {
            try
            {
                return Ok(_requestOverTimeService.GetAllRequestOverTime(nameSearch, status, month, employeeId));
            } catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("cancel-approved-OT-request-for-hr")]
        public async Task<ActionResult<object>> CancelApprovedLeaveRequest(RequestReasonDTO request)
        {
            try
            {
                return Ok(await _requestOverTimeRepository.CancelApprovedOvertimeRequest(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("delete-nonapproved-OT-request-for-employee")]
        public async Task<ActionResult<object>> DeleteLeaveRequestIfNotApproved(Guid requestId, Guid? employeeIdDecider)
        {
            try
            {
                return Ok(await _requestOverTimeRepository.DeleteOvertimeRequestIfNotApproved(requestId, employeeIdDecider));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("get-total-hours-month-year-managername")]
        public async Task<ActionResult<dynamic>> GetEmployeeOvertimeSummaryAndManagerName(Guid employeeId)
        {
            try
            {
                return Ok(await _requestOverTimeRepository.GetEmployeeOvertimeSummaryAndManagerName(employeeId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("approve-over-time-request")]
        public async Task<ActionResult<object>> ApproveOvertimeRequestAndLogHours(Guid requestId, Guid? employeeIdDecider)
        {
            try
            {
                return Ok(await _requestOverTimeRepository.ApproveOvertimeRequestAndLogHours(requestId, employeeIdDecider));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPatch("reject-request-over-time")]
        public async Task<ActionResult<object>> RejectOvertimeRequest(RequestReasonDTO requestObj)
        {
            try
            {
                return Ok(await _requestOverTimeRepository.RejectOvertimeRequest(requestObj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete-request-overtime-by-request-id")]
        public async Task<ActionResult<object>> DeleteOvertimeRequest(Guid requestId)
        {
            try
            {
                return Ok(await _requestOverTimeRepository.DeleteOvertimeRequest(requestId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-request-overtime-by-request-id")]
        public async Task<ActionResult<RequestOverTimeDTO>> GetRequestOverTimeById(Guid requestId)
        {
            try
            {
                return Ok(await _requestOverTimeRepository.GetRequestOverTimeById(requestId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

