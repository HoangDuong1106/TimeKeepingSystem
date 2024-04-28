using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.DTO;
using BusinessObject.Model;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FakeDataController : ControllerBase
    {
        private readonly IWifiService _wifiService;
        private readonly IUserAccountRepository _repositoryAccount;
        private readonly IWorkslotEmployeeRepository _workslotEmployeeRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public FakeDataController(IWifiService wifiService, IUserAccountRepository userAccountRepository, IWorkslotEmployeeRepository workslotEmployeeRepository, IEmployeeRepository employeeRepository)
        {
            _wifiService = wifiService;
            _repositoryAccount = userAccountRepository;
            _workslotEmployeeRepository = workslotEmployeeRepository;
            _employeeRepository = employeeRepository;
        }

        //[HttpGet("create-multiple-employee-account-of-department")]
        //public async Task<ActionResult<List<object>>> CreateMultiple(Guid DepartmentId)
        //{
        //    try
        //    {
        //        return Ok(await _employeeRepository.CreateMultipleEmployee(DepartmentId));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest("Fail");
        //    }
        //}


        [HttpPatch("generate-checkin-checkout-workslot-employee")]
        public async Task<ActionResult<object>> CheckInOutForPeriod(Guid departmentId, string startDateStr, string endDateStr)
        {
            try
            {
                // Convert the string dates to DateTime
                DateTime startDate = DateTime.ParseExact(startDateStr, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(endDateStr, "yyyy/MM/dd", CultureInfo.InvariantCulture);

                // Pass the converted DateTime values to the CheckInOutForPeriod method

                return Ok(await _workslotEmployeeRepository.CheckInOutForPeriod(departmentId, startDate, endDate));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
