using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace TimeKeepingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkSettingController : ControllerBase
    {
        private readonly IUserAccountRepository repositoryAccount;
        private readonly IConfiguration configuration;
        public WorkSettingController(IUserAccountRepository _repositoryAccount, IConfiguration configuration)
        {
            repositoryAccount = _repositoryAccount;
            this.configuration = configuration;
        }
        [HttpGet("GetRiskSetting")]
        public async Task<IActionResult> GetRiskPerFormanceSettingByDepartment(Guid Id)
        {

            try
            {
                var AccountList = await repositoryAccount.GetRiskPerFormanceSettingByDepartment(Id);

                return Ok(new { StatusCode = 200, Message = "Load successful", data = AccountList });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }
        [HttpGet("GetDateSetting")]
        public async Task<IActionResult> GetWorkDateSettingByDepartment(Guid Id)
        {

            try
            {
                var AccountList = await repositoryAccount.GetWorkDateSettingByDepartment(Id);

                return Ok(new { StatusCode = 200, Message = "Load successful", data = AccountList });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }
        [HttpGet("GetTimeSetting")]
        public async Task<IActionResult> GetWorkTimeSettingByDepartment(Guid Id)
        {

            try
            {
                var AccountList = await repositoryAccount.GetWorkTimeSettingByDepartment(Id);

                return Ok(new { StatusCode = 200, Message = "Load successful", data = AccountList });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }
        [HttpGet("GetLeaveSetting")]
        public async Task<IActionResult> GetLeaveSettingByDepartment(Guid Id)
        {

            try
            {
                var AccountList = await repositoryAccount.GetLeaveSettingByDepartment(Id);

                return Ok(new { StatusCode = 200, Message = "Load successful", data = AccountList });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }
        [HttpPut("UpdateTimeSetting")]
        public async Task<IActionResult> UpdateWorkTimeSettingByDepartment(WorkTimeSetting acc)
        {

            try
            {
                var newAcc = new WorkTimeSetting
                {
                    Id = acc.Id,
                    IsDeleted = acc.IsDeleted,
                    FromHourAfternoon = acc.FromHourAfternoon,
                    FromHourMorning = acc.FromHourMorning,
                    ToHourAfternoon = acc.ToHourAfternoon,
                    ToHourMorning = acc.ToHourMorning

                };
                await repositoryAccount.UpdateWorkTimeSetting(newAcc);

                return Ok(new { StatusCode = 200, Message = "Update Successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }

        [HttpGet("get-all-work-time-setting")]
        public async Task<IActionResult> GetAllWorkTimeSetting()
        {
            try
            {
                return Ok(await repositoryAccount.GetAllWorkTimeSetting());
            } catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }


        [HttpPut("UpdateLeaveSetting")]
        public async Task<IActionResult> UpdateLeaveSettingByDepartment(LeaveSetting acc)
        {

            try
            {
                var newAcc = new LeaveSetting
                {
                    LeaveSettingId = acc.LeaveSettingId,
                    IsDeleted = acc.IsDeleted,
                    IsManagerAssigned = acc.IsManagerAssigned,
                    MaxDateLeave = null,

                };
                await repositoryAccount.UpdateLeaveSetting(newAcc);

                return Ok(new { StatusCode = 200, Message = "Update Successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }
        [HttpPut("updateRiskSetting")]
        public async Task<IActionResult> UpdateRiskSettingByDepartment(RiskPerformanceSetting acc)
        {

            try
            {
                var newAcc = new RiskPerformanceSetting
                {
                    Id = (Guid)acc.Id,
                    IsDeleted = (bool)acc.IsDeleted,
                    DateSet = DateTime.Now,
                    Days = acc.Days,
                    Hours = acc.Hours
                };
                await repositoryAccount.UpdateRiskPerFormanceSetting(newAcc);

                return Ok(new { StatusCode = 200, Message = "Update Successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }
    }
}
