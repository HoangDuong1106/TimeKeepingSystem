using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        private readonly MyDbContext _dbContext;

        public EmployeeRepository(MyDbContext context) : base(context)
        {
            // You can add more specific methods here if needed
            _dbContext = context;
        }

        public async Task<List<EmployeeDTO>> GetAllAsync(Guid? roleId, Guid? DepartmentID, string? Searchname)
        {
            var employees = await base.GetAllAsync();
            if (roleId != null) employees = employees.Include(e => e.UserAccount).Where(e => e.UserAccount.RoleID == roleId);
            if (DepartmentID != null) employees = employees.Where(e => e.DepartmentId == DepartmentID);
            if (Searchname != null) employees = employees.Where(e => (e.FirstName + " " + e.LastName).ToLower().Contains(Searchname.ToLower()));
            return await employees.Include(e => e.UserAccount).ThenInclude(ua => ua.Role).Include(e => e.Department).Select(a => new EmployeeDTO
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Email = a.Email,
                Address = a.Address,
                Gender = a.Gender,
                PhoneNumber = a.PhoneNumber,
                RoleName = a.UserAccount.Role.Name,
                RoleId = a.UserAccount.RoleID,
                //ManagerId = a.Department.ManagerId,
                DepartmentId = (Guid)(a.DepartmentId ?? null),
                DepartmentName = a.Department.Name,
                EmployeeStatus = (int?)a.EmployeeStatus,
                EmployeeStatusName = a.EmployeeStatus.ToString() ?? "",
                UserID = a.UserID,
                IsDeleted = a.IsDeleted,
                Type = a.EmploymentType
            }).ToListAsync();
        }

        public async Task<EmployeeDTO> GetById(Guid employeeId)
        {
            var employees = await base.GetAllAsync();
            return employees.Include(e => e.UserAccount).ThenInclude(ua => ua.Role).Include(e => e.Department).Where(e => e.Id == employeeId).Select(a => new EmployeeDTO
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Email = a.Email,
                Address = a.Address,
                Gender = a.Gender,
                PhoneNumber = a.PhoneNumber,
                RoleName = a.UserAccount.Role.Name,
                RoleId = a.UserAccount.RoleID,
                //ManagerId = a.Department.ManagerId,
                DepartmentId = (Guid)(a.DepartmentId ?? null),
                DepartmentName = a.Department.Name,
                EmployeeStatus = (int?)a.EmployeeStatus,
                EmployeeStatusName = a.EmployeeStatus.ToString() ?? "",
                UserID = a.UserID,
                IsDeleted = a.IsDeleted
            }).FirstOrDefault();
        }

        public async Task<bool> AddAsync(EmployeeDTO a)
        {
            try
            {
                await base.AddAsync(new Employee() // have dbSaveChange inside method
                {
                    Id = (Guid)a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Role = a.RoleInTeam,
                    DepartmentId = a.DepartmentId,
                    UserID = (Guid)a.UserID,
                    IsDeleted = (bool)a.IsDeleted
                });
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            try
            {
                await base.SoftDeleteAsync(id);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<object> CreateEmployee(EmployeeDTO newEmployeeDTO)
        {
            try
            {
                // Map EmployeeDTO to Employee
                Employee newEmployee = new Employee()
                {
                    Id = Guid.NewGuid(),
                    FirstName = newEmployeeDTO.FirstName,
                    LastName = newEmployeeDTO.LastName,
                    Email = newEmployeeDTO.Email,
                    Address = newEmployeeDTO.Address ?? null,
                    Gender = (bool)newEmployeeDTO.Gender,
                    Role = newEmployeeDTO.RoleInTeam,
                    PhoneNumber = newEmployeeDTO.PhoneNumber,
                    DepartmentId = newEmployeeDTO.DepartmentId ?? null,
                    Department = newEmployeeDTO.DepartmentId != null ? _dbContext.Departments.FirstOrDefault(d => d.Id == newEmployeeDTO.DepartmentId) : null,
                    //UserID = newEmployeeDTO.UserID,
                    // Add other fields here
                    EmployeeStatus = EmployeeStatus.Working,
                    IsDeleted = false
                };

                // Call the AddAsync method from the repository to save the new employee
                _dbContext.Employees.Add(newEmployee);
                await _dbContext.SaveChangesAsync();
                return new
                {
                    EmployeeId = newEmployee.Id
                };
            }
            catch (Exception ex)
            {
                // Log the exception and return false
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<object> EditEmployee(EmployeeDTO employeeDTO)
        {
            try
            {
                // Find the existing employee by ID
                Employee existingEmployee = _dbContext.Employees.Include(e => e.UserAccount).FirstOrDefault(e => e.Id == employeeDTO.Id);

                // If the employee does not exist, return an appropriate message
                if (existingEmployee == null)
                {
                    throw new Exception("employeeID " + employeeDTO.Id + " Not Found");
                }

                // Update the employee fields from the DTO if they are not null
                if (employeeDTO.FirstName != null)
                {
                    existingEmployee.FirstName = employeeDTO.FirstName;
                }

                if (employeeDTO.LastName != null)
                {
                    existingEmployee.LastName = employeeDTO.LastName;
                }

                if (employeeDTO.Email != null)
                {
                    existingEmployee.Email = employeeDTO.Email;
                }

                if (employeeDTO.Address != null)
                {
                    existingEmployee.Address = employeeDTO.Address;
                }

                if (employeeDTO.Gender != null)
                {
                    existingEmployee.Gender = (bool)employeeDTO.Gender;
                }

                if (employeeDTO.RoleInTeam != null)
                {
                    existingEmployee.Role = employeeDTO.RoleInTeam;
                }

                if (employeeDTO.PhoneNumber != null)
                {
                    existingEmployee.PhoneNumber = employeeDTO.PhoneNumber;
                }

                if (employeeDTO.DepartmentId != null)
                {
                    existingEmployee.DepartmentId = employeeDTO.DepartmentId;
                    existingEmployee.Department = _dbContext.Departments.FirstOrDefault(d => d.Id == employeeDTO.DepartmentId);
                }

                if (employeeDTO.IsDeleted != null)
                {
                    existingEmployee.IsDeleted = (bool)employeeDTO.IsDeleted;
                }

                if (employeeDTO.EmployeeStatus != null)
                {
                    existingEmployee.EmployeeStatus = employeeDTO.EmployeeStatus == 0 ? EmployeeStatus.WaitForWork : (employeeDTO.EmployeeStatus == 1 ? EmployeeStatus.Working : EmployeeStatus.Leaved);
                }

                if (employeeDTO.RoleId != null)
                {
                    existingEmployee.UserAccount.RoleID = (Guid)employeeDTO.RoleId;
                    existingEmployee.UserAccount.Role = _dbContext.Roles.FirstOrDefault(r => r.ID == (Guid)employeeDTO.RoleId);
                }
                // Update other fields as needed, following the same pattern
                
                // Save the changes
                await _dbContext.SaveChangesAsync();

                return new { Message = "Employee updated successfully" };
            }
            catch (Exception ex)
            {
                // Log the exception and return false
                Console.WriteLine(ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}
