using Microsoft.AspNetCore.Mvc;
using BusinessObject.Model;
using DataAccess.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text.Json;
using DataAccess.InterfaceService;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using System.Globalization;

namespace TimeKeepingSystem.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IUserAccountRepository repositoryAccount;
        private readonly IConfiguration configuration;
        private readonly IWorkDateSettingService _service;
        private readonly ITeamService _departmentService;
        private readonly ITeamRepository _departmentRepository;
        public DepartmentController(IUserAccountRepository _repositoryAccount, IConfiguration configuration, IWorkDateSettingService service, ITeamService departmentService, ITeamRepository departmentRepository)
        {
            repositoryAccount = _repositoryAccount;
            this.configuration = configuration;
            _service = service;
            _departmentService = departmentService;
            _departmentRepository = departmentRepository;
        }

        [HttpGet]
        //[Authorize(Roles = "1")]
        public async Task<IActionResult> GetAll()
        {

            try
            {
                var AccountList = await repositoryAccount.GetDepartments();

                return Ok(new { StatusCode = 200, Message = "Load successful", data = AccountList });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }


        [HttpPost]
        //[Authorize(Roles = "1")]
        public async Task<IActionResult> Create(DepartmentDTO acc)
        {
            try
            {
                Guid leaveId = Guid.NewGuid();
                Guid dateId = Guid.NewGuid();
                Guid timeId = Guid.NewGuid();
                Guid riskId = Guid.NewGuid();
                Guid trackId = Guid.NewGuid();
                DateStatusDTO newDateStatus = new DateStatusDTO
                {
                    Monday = true,
                    Tuesday = true,
                    Thursday = true,
                    Wednesday = true,
                    Friday = true,
                    Saturday = false,
                    Sunday = false
                };
                string dateStatusJson = newDateStatus != null ? JsonSerializer.Serialize<DateStatusDTO>(newDateStatus) : null;
                WorkDateSetting newWorkDateSetting = new WorkDateSetting
                {
                    Id = dateId,
                    IsDeleted = false,
                    DateStatus = dateStatusJson
                };
                await repositoryAccount.AddWorkDateSetting(newWorkDateSetting);
                var newTimeSetting = new WorkTimeSetting
                {
                    Id = timeId,
                    IsDeleted = false,
                    FromHourMorning = "08:00",
                    ToHourMorning = "12:00",
                    FromHourAfternoon = "13:00",
                    ToHourAfternoon = "17:00"
                };
                await repositoryAccount.AddWorkTimeSetting(newTimeSetting);
                var newRiskSetting = new RiskPerformanceSetting
                {
                    Id = riskId,
                    IsDeleted = false,
                    Days = 4,
                    Hours = 30,
                    DateSet = DateTime.Now,
                };
                await repositoryAccount.AddRiskPerFormanceSetting(newRiskSetting);
                var newList = await repositoryAccount.GetLeaveSettings();
                
                var oldeaveSetting = newList.FirstOrDefault(ls => ls.LeaveSettingId == Guid.Parse("24D6E45C-5551-11EE-8C99-0242AC120002"));
                var newLeaveSetting = new LeaveSetting()
                {
                    LeaveSettingId = leaveId,
                    IsManagerAssigned = oldeaveSetting.IsManagerAssigned,
                    MaxDateLeave = oldeaveSetting.MaxDateLeave,
                    IsDeleted = false
                };

                await repositoryAccount.AddLeaveSetting(newLeaveSetting);
                var newWorkSetting = new WorkTrackSetting
                {
                    Id = trackId,
                    IsDeleted = false,
                    LeaveSettingId = leaveId,
                    RiskPerfomanceId = riskId,
                    WorkDateId = dateId,
                    WorkTimeId = timeId,
                };
                var a = await repositoryAccount.GetWorkTimeSettings();
                var b = await repositoryAccount.GetRiskPerFormanceSettings();
                var c = await _service.GetAll();

                await repositoryAccount.AddWorkTrackSetting(newWorkSetting);
                Guid id = Guid.NewGuid();
                var newAcc = new Team
                {
                    Id = id,
                    //ManagerId = Guid.Parse("57076183-1d8d-43b1-a6ff-17cd4f4b71e1"),
                    IsDeleted = false,
                    Name = acc.Name,
                    WorkTrackId = trackId,

                };
                await repositoryAccount.AddDepartment(newAcc);
                return Ok(new { StatusCode = 200, Message = "Add successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new
                {
                    StatusCode = 409,
                    Message = ex.Message
                });
            }
        }


        [HttpPut]
        public async Task<IActionResult> Update(DepartmentDTO acc)
        {
            try
            {
                Guid id = Guid.NewGuid();
                var newAcc = new Team
                {
                    Id = (Guid)acc.Id,
                    //ManagerId = Guid.Parse("57076183-1d8d-43b1-a6ff-17cd4f4b71e1"),
                    IsDeleted = false,
                    Name = acc.Name,
                    WorkTrackId = Guid.Parse("298F2708-4C63-11EE-BE56-0242AC120002"),

                };
                await repositoryAccount.UpdateDepartment(newAcc);
                return Ok(new { StatusCode = 200, Message = "Update successful" });

            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }


        [HttpDelete("{id}")]
        //[Authorize(Roles = "1")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await repositoryAccount.DeleteDepartment(id);

                return Ok(new { StatusCode = 200, Message = "Delete successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }

        [HttpGet("get-all-employee-in-department-id")]
        public async Task<List<EmployeeDTO>> GetEmployeesByDepartmentIdAsync(Guid departmentId)
        {
            return await _departmentService.GetEmployeesByDepartmentIdAsync(departmentId);
        }

        [HttpGet("get-department-without-manager")]
        public List<DepartmentDTO> GetDepartmentsWithoutManager()
        {
            return _departmentService.GetDepartmentsWithoutManager();
        }

        [HttpGet("get-department-info-by-employee-id")]
        public Task<object> GetTeamInfoByEmployeeIdAsync(Guid employeeId)
        {
            return _departmentRepository.GetTeamInfoByEmployeeIdAsync(employeeId);
        }

        [HttpPut("update-teammember-role-info-of-department")]
        public async Task<ActionResult<object>> UpdateTeamInformation(TeamUpdateDTO data)
        {
            try
            {
                return Ok(await _departmentRepository.UpdateTeamInformation(data));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

