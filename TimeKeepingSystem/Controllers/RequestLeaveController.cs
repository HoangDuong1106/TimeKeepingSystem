
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TimeKeepingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestLeaveController : ControllerBase
    {
        private readonly IAttendanceStatusService _service;
        private readonly IRequestLeaveService _requestLeaveService;
        private readonly IRequestLeaveRepository _requestLeaveRepository;
        private readonly IRequestRepository _requestRepository;

        public RequestLeaveController(IAttendanceStatusService service, IRequestLeaveService requestLeaveService, IRequestRepository requestRepository, IRequestLeaveRepository requestLeaveRepository)
        {
            _service = service;
            _requestLeaveService = requestLeaveService;
            _requestRepository = requestRepository;
            _requestLeaveRepository = requestLeaveRepository;
        }

        [HttpGet("get-request-leave-of-employee")]
        public ActionResult<object> GetRequestLeaveOfEmployeeById(Guid employeeId)
        {
            try
            {
                return Ok(_requestLeaveService.GetRequestLeaveOfEmployeeById(employeeId));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-work-date-setting-of-employee")]
        public async Task<ActionResult<WorkDateSettingDTO>> GetWorkDateSettingFromEmployeeId(Guid employeeId)   
        {
            try
            {
                return Ok(await _requestLeaveService.GetWorkDateSettingFromEmployeeId(employeeId));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("edit-request-leave-of-employee")]
        public async Task<ActionResult<bool>> EditRequestLeave(LeaveRequestDTO dto, Guid employeeId)
        {
            try
            {
                return Ok(await _requestLeaveService.EditRequestLeave(dto, employeeId));
            } catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create-request-leave-of-employee")]
        public async Task<ActionResult<object>> CreateRequestLeave(LeaveRequestDTO dto, Guid employeeId)
        {
            try
            {
                return Ok(await _requestLeaveService.CreateRequestLeave(dto, employeeId));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-all-request-leave-of-all-employee")]
        public ActionResult<object> GetRequestLeaveAllEmployee(string? nameSearch, int status)
        {
            try
            {
                return Ok(_requestLeaveService.GetRequestLeaveAllEmployee(nameSearch, status));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("approve-leave-request")]
        public async Task<IActionResult> ApproveRequestAndChangeWorkslotEmployee(Guid requestId, Guid? employeeIdDecider)
        {
            try
            {
                return Ok(await _requestLeaveService.ApproveRequestAndChangeWorkslotEmployee(requestId, employeeIdDecider));
            } catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("get-request-leave-by-request-id")]
        public IActionResult GetRequestLeaveByRequestId(Guid requestId)
        {
            try
            {
                return Ok(_requestLeaveService.GetRequestLeaveByRequestId(requestId));
            } catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("get-number-date-leave-each-leave-type-of-employee")]
        public async Task<ActionResult<object>> GetApprovedLeaveDaysByTypeAsync(Guid employeeId)
        {
            try
            {
                return Ok(await _requestLeaveRepository.GetApprovedLeaveDaysByTypeAsync(employeeId));         
                    
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("cancel-approved-leave-request-for-hr")]
        public async Task<ActionResult<object>> CancelApprovedLeaveRequest(RequestReasonDTO request)
        {
            try
            {
                return Ok(await _requestLeaveRepository.CancelApprovedLeaveRequest(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("delete-nonapproved-leave-request-for-employee")]
        public async Task<ActionResult<object>> DeleteLeaveRequestIfNotApproved(Guid requestId, Guid? employeeIdDecider)
        {
            try
            {
                return Ok(await _requestLeaveRepository.DeleteLeaveRequestIfNotApproved(requestId, employeeIdDecider));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("send-firebase-request-data-status")]
        public async Task<ActionResult<object>> SendLeaveRequestStatusToFirebase(Guid requestId)
        {
            try
            {
                return Ok(await _requestLeaveRepository.SendLeaveRequestStatusToFirebase(requestId, ""));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

