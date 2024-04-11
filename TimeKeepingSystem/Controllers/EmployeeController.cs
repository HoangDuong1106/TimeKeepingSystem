
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using Microsoft.AspNetCore.Mvc;

namespace TimeKeepingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IAttendanceStatusService _service;
        private readonly IEmployeeService _employeeService;
        private readonly IEmployeeRepository _employeeRepo;

        public EmployeeController(IAttendanceStatusService service, IEmployeeService employeeService, IEmployeeRepository employeeRepository)
        {
            _service = service;
            _employeeService = employeeService;
            _employeeRepo = employeeRepository;
        }

        // GET: api/AttendanceStatus
        [HttpGet]
        public async Task<ActionResult<List<EmployeeDTO>>> GetAllAsync(Guid? roleId, Guid? DepartmentID, string? Searchname)
        {
            return Ok(await _employeeService.GetAllAsync(roleId, DepartmentID, Searchname));
        }

        // GET: api/AttendanceStatus/{id}
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetById(Guid id)
        //{
        //    var entity = await _service.GetByIdAsync(id);
        //    if (entity == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(entity);
        //}

        // POST: api/AttendanceStatus
        [HttpPost]
        public async Task<ActionResult<bool>> AddAsync(EmployeeDTO dto)
        {
            return Ok(await _employeeService.AddAsync(dto));
        }

        //// PUT: api/AttendanceStatus
        //[HttpPut]
        //public async Task<IActionResult> Update(AttendanceStatus entity)
        //{
        //    return Ok(await _service.UpdateAsync(entity));
        //}

        // DELETE: api/AttendanceStatus/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> SoftDeleteAsync(Guid id)
        {
            var entity = await _employeeService.SoftDeleteAsync(id);
            if (entity == null)
            {
                return NotFound();
            }
            return Ok(entity);
        }

        [HttpPost("create-employee")]
        public async Task<ActionResult<object>> CreateEmployee([FromBody]EmployeeDTO newEmployeeDTO)
        {
            try
            {
                return Ok(await _employeeService.CreateEmployee(newEmployeeDTO));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-employee-by-id")]
        public async Task<ActionResult<object>> GetById(Guid employeeId)
        {
            try
            {
                return Ok(await _employeeService.GetById(employeeId));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("edit-employee-by-id")]
        public async Task<ActionResult<object>> EditEmployee([FromBody]EmployeeDTO employeeDTO)
        {
            try
            {
                return Ok(await _employeeService.EditEmployee(employeeDTO));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("remote-and-demote-employee-by-id")]
        public async Task<ActionResult<string>> ChangeEmployeeRoleAsync(Guid employeeId)
        {
            try
            {
                return Ok(await _employeeRepo.ChangeEmployeeRoleAsync(employeeId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("create-email-from-username-for-nonexisting-email-employee")]
        public async Task<ActionResult<List<Guid>>> CheckForDuplicateEmailsAndUpdateAsync()
        {
            try
            {
                return Ok(await _employeeRepo.CheckForDuplicateEmailsAndUpdateAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

