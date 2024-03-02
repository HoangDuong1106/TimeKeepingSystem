
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using Microsoft.AspNetCore.Mvc;

namespace TimeKeepingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceStatusController : ControllerBase
    {
        private readonly IAttendanceStatusService _service;
        private readonly IWorkingStatusRepository _workingStatusRepository;

        public AttendanceStatusController(IAttendanceStatusService service, IWorkingStatusRepository workingStatusRepository)
        {
            _service = service;
            _workingStatusRepository = workingStatusRepository;
        }

        // GET: api/AttendanceStatus
        [HttpGet]
        public async Task<ActionResult<List<AttendanceStatusDTO>>> GetAllAsync()
        {
            return Ok(await _service.GetAllAsync());
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
        public async Task<ActionResult<bool>> AddAsync(AttendanceStatusDTO dto)
        {
            return Ok(await _service.AddAsync(dto));
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
            var entity = await _service.SoftDeleteAsync(id);
            if (entity == null)
            {
                return NotFound();
            }
            return Ok(entity);
        }

        [HttpPost("create-working-status-attendance")]
        public async Task<ActionResult<object>> CreateWorkingStatus(WorkingStatusDTO workingStatusDTO)
        {
            try
            {
                return Ok(await _workingStatusRepository.Create(workingStatusDTO));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

