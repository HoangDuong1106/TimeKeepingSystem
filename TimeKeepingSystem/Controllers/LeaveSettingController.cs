using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using BusinessObject.Model;
using DataAccess.InterfaceService;
using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveSettingController : ControllerBase
    {
        
        private readonly ILeaveSettingService _leaveSettingService;

        public LeaveSettingController(ILeaveSettingService leaveSettingService)
        {
            _leaveSettingService = leaveSettingService;
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateLeaveSettingAsync([FromBody]LeaveSettingDTO updatedLeaveSetting)
        {
            try
            {
                return Ok(await _leaveSettingService.UpdateLeaveSettingAsync(updatedLeaveSetting));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                return Ok(await _leaveSettingService.GetAllAsync());
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
