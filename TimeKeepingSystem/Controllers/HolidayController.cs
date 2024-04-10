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

namespace TimeKeepingSystem.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class HolidayController : ControllerBase
    {
        private readonly IUserAccountRepository repositoryAccount;
        private readonly IConfiguration configuration;
        private readonly IDepartmentHolidayRepository _departmentHolidayRepository;
        public HolidayController(IUserAccountRepository _repositoryAccount, IConfiguration configuration, IDepartmentHolidayRepository departmentHolidayRepository)
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
                Guid id = Guid.NewGuid();
                var newAcc = new DepartmentHoliday
                {
                    HolidayId = (Guid)acc.HolidayId,
                    HolidayName = acc.HolidayName,
                    Description = acc.Description,
                    IsDeleted = false,
                    IsRecurring = (bool)((acc.IsRecurring != null) ? acc.IsRecurring : true),
                    StartDate = acc.StartDate,
                    EndDate = acc.EndDate,
                };
                await repositoryAccount.UpdateHoliday(newAcc);
                return Ok(new { StatusCode = 200, Message = "Update successful" });

            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }

        [HttpDelete]

        public async Task<IActionResult> Delete(Guid[] id)
        {
            try
            {
                foreach (Guid departmentId in id)
                {
                    await repositoryAccount.DeleteHoliday(departmentId);
                }
                return Ok(new { StatusCode = 200, Message = "Delete successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }


        }
    }
}

