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

        public FakeDataController(IWifiService wifiService, IUserAccountRepository userAccountRepository, IWorkslotEmployeeRepository workslotEmployeeRepository)
        {
            _wifiService = wifiService;
            _repositoryAccount = userAccountRepository;
            _workslotEmployeeRepository = workslotEmployeeRepository;
        }

        private List<EmployeeDTO> ReadCsvFile(IFormFile csvFile)
        {
            List<EmployeeDTO> employees = new List<EmployeeDTO>();

            using (var reader = new StreamReader(csvFile.OpenReadStream()))
            {
                // Skip the header row
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    EmployeeDTO employee = new EmployeeDTO
                    {
                        FirstName = values[0].Trim(),
                        LastName = values[1].Trim(),
                        Email = values[2].Trim(),
                        Address = values[3].Trim(),
                        Gender = values[4].Trim().ToUpper() == "TRUE",
                        PhoneNumber = values[5].Trim(),
                        UserName = values[6].Trim(),
                        Password = values[7].Trim()
                    };

                    employees.Add(employee);
                }
            }

            return employees;
        }

        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        public static string GenerateHashedPassword(string password, string salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltBytes = Convert.FromBase64String(salt);

            byte[] hashedPasswordBytes;
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordWithSaltBytes = new byte[passwordBytes.Length + saltBytes.Length];
                Buffer.BlockCopy(passwordBytes, 0, passwordWithSaltBytes, 0, passwordBytes.Length);
                Buffer.BlockCopy(saltBytes, 0, passwordWithSaltBytes, passwordBytes.Length, saltBytes.Length);

                hashedPasswordBytes = sha256.ComputeHash(passwordWithSaltBytes);
            }

            return Convert.ToBase64String(hashedPasswordBytes);
        }

        [HttpPost("create-multiple-employee-account-of-department")]
        //[Authorize(Roles = "1")]
        public async Task<IActionResult> CreateMultiple(Guid DepartmentId)
        {
            try
            {
                // Read the Excel file and convert it to a list of Employee objects
                List<EmployeeDTO> employees = new List<EmployeeDTO>
{
    new EmployeeDTO { FirstName = "Robert", LastName = "Jones", Email = "robdert.jones@example.com", Address = "303 Cedar St", Gender = true, PhoneNumber = "678-901-2345", UserName = "robert", Password = "123" },
    new EmployeeDTO { FirstName = "Alice", LastName = "Garcia", Email = "alicde.garcia@example.com", Address = "404 Birch St", Gender = false, PhoneNumber = "789-012-3456", UserName = "alice", Password = "123" },
    new EmployeeDTO { FirstName = "David", LastName = "Martinez", Email = "ddavid.martinez@example.com", Address = "505 Redwood St", Gender = true, PhoneNumber = "890-123-4567", UserName = "david", Password = "123" }
    
};

                foreach (var emp in employees)
                {
                    // Generate salt and hashed password
                    var saltPassword = GenerateSalt();
                    var hashPassword = GenerateHashedPassword(emp.Password, saltPassword);
                    var userId = Guid.NewGuid();
                    var employeeId = Guid.NewGuid();
                    // Create new Employee and UserAccount objects
                    var newEmployee = new Employee
                    {
                        Id = employeeId,
                        FirstName = emp.FirstName,
                        Email = emp.Email,
                        Address = emp.Address,
                        Gender = (bool)emp.Gender,
                        IsDeleted = false,
                        LastName = emp.LastName,
                        PhoneNumber = emp.PhoneNumber,
                        Role = "Employee",
                        UserID = userId,
                        EmployeeStatus = EmployeeStatus.Working,
                        DepartmentId = DepartmentId,
                        //Team = await _departmentService.GetDepartmentAsync(DepartmentId)
                    };
                    await _repositoryAccount.AddEmployee(newEmployee);

                    var newUserAccount = new UserAccount
                    {
                        // Populate fields and set other properties
                        ID = userId,
                        Username = emp.UserName,
                        SaltPassword = saltPassword,
                        PasswordHash = hashPassword,
                        EmployeeId = employeeId,
                        IsActive = true,
                        RoleID = Guid.Parse("C43450F8-4D7B-11EE-BE56-0242AC120002"),
                        IsDeleted = false
                    };
                    await _repositoryAccount.AddMember(newUserAccount);

                }

                return Ok(new { StatusCode = 200, Message = "Add successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }

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
