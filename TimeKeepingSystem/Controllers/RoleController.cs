
using BusinessObject.DTO;
using DataAccess.InterfaceService;
using Microsoft.AspNetCore.Mvc;

namespace TimeKeepingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _service;

        public RoleController(IRoleService service)
        {
            _service = service;
        }

        // GET: api/AttendanceStatus
        [HttpGet]
        public async Task<ActionResult<object>> GetAllAsync()
        {
            return Ok(await _service.GetAllRole());
        }

    }
}

