
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Service;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace TimeKeepingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkSlotEmployeeController : ControllerBase
    {
        private readonly IWorkslotEmployeeService _service;
        private readonly IWorkslotEmployeeRepository _workslotEmployeeRepository;

        public WorkSlotEmployeeController(IWorkslotEmployeeService service, IWorkslotEmployeeRepository workslotEmployeeRepository)
        {
            _service = service;
            _workslotEmployeeRepository = workslotEmployeeRepository;
        }

        // GET: api/AttendanceStatus
        [HttpPost("generate-workslot-for-all-employee-in-department")]
        public async Task<object> GenerateWorkSlotEmployee(CreateWorkSlotRequest request)
        {
            return await _workslotEmployeeRepository.GenerateWorkSlotEmployee(request);
        }

        [HttpGet("get-workslot-employee-by-employee-id")]
        public async Task<object> GetWorkSlotEmployeeByEmployeeId(Guid employeeId)
        {
            return await _workslotEmployeeRepository.GetWorkSlotEmployeeByEmployeeId(employeeId);
        }

        [HttpGet("get-workslot-employee-by-department-id")]
        public async Task<object> GetWorkSlotEmployeesByDepartmentId(Guid departmentId, string startTime, string endTime)
        {
            return await _workslotEmployeeRepository.GetWorkSlotEmployeesByDepartmentId(departmentId, startTime, endTime);
        }

        [HttpGet("export-excel-file")]
        public async Task<IActionResult> ExportWorkSlotEmployeeReport(Guid departmentId)
        {
            try
            {
                string relativePath = await _workslotEmployeeRepository.ExportWorkSlotEmployeeReport(departmentId);
                string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath.TrimStart('.').TrimStart('/'));
                string fileName = Path.GetFileName(absolutePath);

                // Set the content type based on the file extension
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return PhysicalFile(absolutePath, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("check-in-workslot-employee-by-employee-id")]
        public async Task<IActionResult> CheckInWorkslotEmployee(Guid employeeId, string? FakecurrentTime)
        {
            try
            {
                if (FakecurrentTime != null)
                {
                    DateTime fakeTime = DateTime.ParseExact(FakecurrentTime, "yyyy,MM,dd,HH,mm,ss", CultureInfo.InvariantCulture);
                    return Ok(await _workslotEmployeeRepository.CheckInWorkslotEmployee(employeeId, fakeTime));
                }
                return Ok(await _workslotEmployeeRepository.CheckInWorkslotEmployee(employeeId, null));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("check-out-workslot-employee-by-employee-id")]
        public async Task<IActionResult> CheckOutWorkslotEmployee(Guid employeeId, string? FakecurrentTime)
        {
            try
            {
                if (FakecurrentTime != null)
                {
                    DateTime fakeTime = DateTime.ParseExact(FakecurrentTime, "yyyy,MM,dd,HH,mm,ss", CultureInfo.InvariantCulture);
                    return Ok(await _workslotEmployeeRepository.CheckOutWorkslotEmployee(employeeId, fakeTime));
                }
                return Ok(await _workslotEmployeeRepository.CheckOutWorkslotEmployee(employeeId, null));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-workslot-employee-today")]
        public async Task<ActionResult<object>> GetWorkSlotEmployeeByEmployeeIdForToday(Guid employeeId)
        {
            try
            {
                return Ok(await _workslotEmployeeRepository.GetWorkSlotEmployeeByEmployeeIdForToday(employeeId));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}

