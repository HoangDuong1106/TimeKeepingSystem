using Microsoft.AspNetCore.Mvc;
using BusinessObject.Model;
using DataAccess.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using OfficeOpenXml;
using DataAccess.InterfaceService;
using BusinessObject.DTO;
using DataAccess.InterfaceRepository;

namespace TimeKeepingSystem.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserAccountRepository repositoryAccount;
        private readonly IConfiguration configuration;
        private readonly ITeamService _departmentService;
        public AccountController(IUserAccountRepository _repositoryAccount, IConfiguration configuration, ITeamService departmentService)
        {
            repositoryAccount = _repositoryAccount;
            this.configuration = configuration;
            _departmentService = departmentService;
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
        private static string GenerateVerificationCode()
        {
            // T?o mã xác th?c ng?u nhiên (ví d?: 6 ch? s?)
            Random random = new Random();
            int code = random.Next(100000, 999999);
            return code.ToString();
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

        [HttpGet]
        //[Authorize(Roles = "1")]
        public async Task<IActionResult> GetAll()
        {

            try
            {
                var AccountList = await repositoryAccount.GetMembers();
                var Count = AccountList.Count();
                return Ok(new { StatusCode = 200, Message = "Load successful", data = AccountList, Count });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }



        [HttpPost("Login")]
        public async Task<ActionResult> GetLogin(LoginDTO acc)
        {
            try
            {

                UserAccount customer = await repositoryAccount.LoginMember(acc.Email, acc.Password);
                if (customer != null)
                {

                    if (customer.IsDeleted == false && (customer.Employee.Department != null || customer.Role.Name == "Admin"))
                    {

                        return Ok(new { StatusCode = 200, Message = "Login succedfully", data = GenerateToken(customer), avatar = customer.Employee.LastName, employeeName = customer.Employee.FirstName + " " + customer.Employee.LastName, role = customer.Role.Name, employeeId = customer.EmployeeId, teamId = customer.Employee.DepartmentId, teamName = customer.Employee.Department?.Name });
                    }
                    else
                    {
                        return Ok(new { StatusCode = 409, Message = "Account Not Active" });
                    }



                }
                else
                {
                    return Ok(new { StatusCode = 409, Message = "Email or Password is valid" });
                }


            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = 409, Message = ex.Message });
            }
        }
        private string GenerateToken(UserAccount acc)
        {

            var secretKey = configuration.GetSection("AppSettings").GetSection("SecretKey").Value;

            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]{
                    new Claim(ClaimTypes.Name, acc.Username),
                    new Claim("employeeId", acc.EmployeeId.ToString()),
                    new Claim("Id", acc.ID.ToString()),
                    new Claim("RoleName", acc.Role.Name),
                    new Claim("TokenId", Guid.NewGuid().ToString()),
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha512Signature)
            };
            var token = jwtTokenHandler.CreateToken(tokenDescription);

            return jwtTokenHandler.WriteToken(token);
        }
        [HttpPost]
        //[Authorize(Roles = "1")]
        public async Task<IActionResult> Create(AccountDTO acc)
        {
            try
            {
                List<Employee> accountList = await repositoryAccount.GetEmployees();


                var saltPassword = GenerateSalt();
                var hashPassword = GenerateHashedPassword(acc.Password, saltPassword);

                var AccountList = await repositoryAccount.GetEmployees();
                Guid id = Guid.NewGuid();

                var newAcc = new Employee
                {
                    Id = id,
                    FirstName = acc.FirstName,
                    Email = acc.Username,
                    Address = acc.Address,
                    Gender = (bool)acc.Gender,
                    IsDeleted = false,
                    LastName = acc.LastName,
                    PhoneNumber = acc.PhoneNumber,
                    Role = "Employee",
                    UserID = id,
                    EmployeeStatus = EmployeeStatus.Working,
                };

                if (acc.DepartmentID != null)
                {
                    newAcc.DepartmentId = (Guid)acc.DepartmentID;
                }
                await repositoryAccount.AddEmployee(newAcc);
                Employee userAccount = accountList.LastOrDefault(x => x.Id != null);
                var newAccount = new UserAccount
                {
                    ID = Guid.NewGuid(),
                    EmployeeId = id,
                    IsActive = true,
                    Username = acc.Username,
                    IsDeleted = false,
                    SaltPassword = saltPassword,
                    PasswordHash = hashPassword,
                    RoleID = (Guid)acc.RoleID,
                };
                await repositoryAccount.AddMember(newAccount);
                return Ok(new { StatusCode = 200, Message = "Add successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }



        [HttpPut("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangPassword(ChangePasswordDTO acc)
        {
            try
            {
                string userIdString = User.FindFirst("Id")?.Value;
                if (Guid.TryParse(userIdString, out Guid userId))
                {
                    UserAccount account = await repositoryAccount.GetProfile(userId);

                    if (account.PasswordHash == null)
                    {
                        await repositoryAccount.ChangePassword(userId, GenerateHashedPassword(acc.NewPassword, account.SaltPassword));
                        return Ok(new { StatusCode = 200, Message = "ChangePassword successful" });
                    }

                    string oldPasswordHash = GenerateHashedPassword(acc.OldPassword, account.SaltPassword);

                    if (oldPasswordHash != account.PasswordHash)
                    {
                        return Ok(new { StatusCode = 400, Message = "Old Password not correct" });
                    }

                    await repositoryAccount.ChangePassword(userId, GenerateHashedPassword(acc.NewPassword, account.SaltPassword));
                    return Ok(new { StatusCode = 200, Message = "ChangePassword successful" });
                }
                else
                {
                    // Handle the case where 'userIdString' is not a valid Guid
                    return Ok(new { StatusCode = 400, Message = "Invalid User ID" });
                }

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
                await repositoryAccount.DeleteMember(id);
                return Ok(new { StatusCode = 200, Message = "Delete successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }


        }

        [HttpPost("create-multiple-employee-account-of-department")]
        //[Authorize(Roles = "1")]
        public async Task<IActionResult> CreateMultiple(IFormFile excelFile, Guid DepartmentId)
        {
            try
            {
                // Read the Excel file and convert it to a list of Employee objects
                List<EmployeeDTO> employees = ReadCsvFile(excelFile);

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
                    await repositoryAccount.AddEmployee(newEmployee);

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
                    await repositoryAccount.AddMember(newUserAccount);
                    
                }

                return Ok(new { StatusCode = 200, Message = "Add successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { StatusCode = 409, Message = ex.Message });
            }
        }

    }
}

