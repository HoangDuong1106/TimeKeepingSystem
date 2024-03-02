
using BusinessObject.DTO;
using DataAccess.InterfaceService;
using DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;

namespace TimeKeepingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkTimeSettingController : ControllerBase
    {
        private readonly IWorkTimeSettingService _service;

        public WorkTimeSettingController(IWorkTimeSettingService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                return Ok(await _service.GetAllAsync());
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            try
            {
                return Ok(await _service.GetByIdAsync(id));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody]WorkTimeSetting workTimeSetting)
        {
            try
            {
                return Ok(await _service.CreateAsync(workTimeSetting));
            } catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(WorkTimeSetting workTimeSetting)
        {
            try
            {
                return Ok(await _service.UpdateAsync(workTimeSetting));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> SoftDeleteAsync(Guid id)
        {
            try
            {
                return Ok(await _service.SoftDeleteAsync(id));
            } catch (Exception ex )
            {
                return BadRequest($"{ex.Message}");
            }
        }

    }
}

