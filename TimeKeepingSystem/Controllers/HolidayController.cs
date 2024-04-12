using Microsoft.AspNetCore.Mvc;
using BusinessObject.Model;
using DataAccess.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;

using DataAccess.InterfaceService;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using System.Globalization;
using DataAccess.DAO;

namespace TimeKeepingSystem.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class HolidayController : ControllerBase
    {
        private readonly IUserAccountRepository repositoryAccount;
        private readonly IConfiguration configuration;
        private readonly IHolidayRepository _departmentHolidayRepository;
        public HolidayController(IUserAccountRepository _repositoryAccount, IConfiguration configuration, IHolidayRepository departmentHolidayRepository)
        {
            repositoryAccount = _repositoryAccount;
            this.configuration = configuration;
            _departmentHolidayRepository = departmentHolidayRepository;
        }

        [HttpGet]
        //[Authorize(Roles = "1")]
        public async Task<IActionResult> GetAll()
        {

            try
            {
                var AccountList = await repositoryAccount.GetHolidays();

                return Ok(new { StatusCode = 200, Message = "Load successful", data = AccountList });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }


        [HttpPost]
        //[Authorize(Roles = "1")]
        public async Task<ActionResult<object>> Create(PostHolidayListDTO acc)
        {
            try
            {
                return Ok(await _departmentHolidayRepository.AddAsync(acc));
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
        public async Task<IActionResult> Update(DepartmentHolidayDTO acc)
        {
            try
            {
                var newAcc = new Holiday
                {
                    HolidayId = (Guid)acc.HolidayId,
                    HolidayName = acc.HolidayName,
                    Description = acc.Description,
                    IsDeleted = false,
                    IsRecurring = (bool)((acc.IsRecurring != null) ? acc.IsRecurring : true),
                    StartDate = acc.StartDate,
                    EndDate = acc.EndDate,
                };
                await Delete((Guid)acc.HolidayId);
                var result = await _departmentHolidayRepository.AddAsync(new PostHolidayListDTO
                {
                    HolidayId = (Guid)acc.HolidayId,
                    HolidayName = acc.HolidayName,
                    Description = acc.Description,
                    IsDeleted = false,
                    IsRecurring = (bool)((acc.IsRecurring != null) ? acc.IsRecurring : true),
                    StartDate = acc.StartDate.ToString("yyyy/MM/dd"),
                    EndDate = acc.EndDate.ToString("yyyy/MM/dd"),
                });
                return Ok(new { StatusCode = 200, result });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                Guid[] ids = { id };
                return Ok(await _departmentHolidayRepository.SoftDelete(ids));
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }

        }
    }
}

