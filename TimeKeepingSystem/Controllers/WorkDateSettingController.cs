
using BusinessObject.DTO;
using DataAccess.InterfaceService;
using Microsoft.AspNetCore.Mvc;

namespace TimeKeepingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkDateSettingController : ControllerBase
    {
        private readonly IWorkDateSettingService _service;

        public WorkDateSettingController(IWorkDateSettingService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<List<WorkDateSettingDTO>> GetAll()
        {
            return await _service.GetAll();
        }

        [HttpPost]
        public async Task<object> Create(WorkDateSettingDTO dto)
        {
            return await _service.Create(dto);
        }

        [HttpGet("get-work-date-setting-by-id")]
        public async Task<WorkDateSettingDTO> GetById(Guid id)
        {
            return await _service.GetById(id);
        }

        [HttpPatch]
        public async Task<bool> Update(WorkDateSettingDTO dto)
        {
            return await _service.Update(dto); 
        }

        [HttpDelete]
        public async Task<bool> Delete(Guid id)
        {
            return await _service.Delete(id);
        }
    }
}

