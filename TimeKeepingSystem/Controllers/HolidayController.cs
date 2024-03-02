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
        public HolidayController(IUserAccountRepository _repositoryAccount, IConfiguration configuration)
        {
            repositoryAccount = _repositoryAccount;
            this.configuration = configuration;
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
        public async Task<IActionResult> Create(PostHolidayListDTO acc)
        {
            try
            {
                foreach (Guid departmentId in acc.DepartmentIds)
                {
                    var newAcc = new DepartmentHoliday
                    {
                        HolidayId = Guid.NewGuid(),
                        HolidayName = acc.HolidayName,
                        DepartmentId = departmentId,
                        Description = acc.Description,
                        IsDeleted = false,
                        IsRecurring = true,
                        StartDate = DateTime.ParseExact( acc.StartDate, "yyyy/MM/dd", CultureInfo.InvariantCulture),
                        EndDate = DateTime.ParseExact(acc.EndDate, "yyyy/MM/dd", CultureInfo.InvariantCulture),

                    };
                    await repositoryAccount.AddHolidayt(newAcc);
                }

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
                    IsRecurring = true,
                    StartDate = acc.StartDate,
                    EndDate = acc.EndDate,
                    DepartmentId = (Guid)acc.DepartmentId,

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

