using BusinessObject.DTO;
using DataAccess.DAO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class DepartmentRepository : Repository<Department>, IDepartmentRepository
    {
        private readonly MyDbContext _dbContext;

        public DepartmentRepository(MyDbContext context) : base(context)
        {
            // You can add more specific methods here if needed
            _dbContext = context;
        }

        public async Task<List<DepartmentDTO>> GetAllAsync()
        {
            var ass = await base.GetAllAsync();
            return await ass.Select(a => new DepartmentDTO
            {
                Id = a.Id,
                //ManagerId = a.ManagerId,
                WorkTrackId = a.WorkTrackId,
                IsDeleted = a.IsDeleted
            }).ToListAsync();
        }

        public async Task<bool> AddAsync(DepartmentDTO a)
        {
            try
            {
                await base.AddAsync(new Department() // have dbSaveChange inside method
                {
                    Id = (Guid)a.Id,
                    //ManagerId = Guid.Parse("57076183-1d8d-43b1-a6ff-17cd4f4b71e1"),
                    WorkTrackId = (Guid)a.WorkTrackId,
                    IsDeleted = false
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

        public async Task<List<EmployeeDTO>> GetEmployeesByDepartmentIdAsync(Guid departmentId)
        {
            try
            {
                // Fetch all employees of the department
                var employees = await _dbContext.Employees.Include(a => a.UserAccount).ThenInclude(ua => ua.Role)
                    .Where(e => e.DepartmentId == departmentId)
                    .ToListAsync();

                var ManagerId = _dbContext.Employees.Include(e => e.UserAccount).ThenInclude(u => u.Role).FirstOrDefault(e => e.DepartmentId == departmentId && e.UserAccount.Role.Name == "Manager") != null ? _dbContext.Employees.Include(e => e.UserAccount).ThenInclude(u => u.Role).FirstOrDefault(e => e.DepartmentId == departmentId && e.UserAccount.Role.Name == "Manager").Id : Guid.Empty;
                

                // Convert to DTOs (assuming you have an EmployeeDTO class)
                var employeeDTOs = employees.Where(em => em.Id != ManagerId).Select(e => new EmployeeDTO
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    RoleName = e.UserAccount != null ? e.UserAccount.Role.Name : null,
                    RoleId = e.UserAccount != null ? e.UserAccount.RoleID : null,
                    Email = e.Email
                    // ... (other fields)
                }).ToList();

                if (ManagerId != Guid.Empty)
                {
                    var manager = _dbContext.Employees.Where(e => e.Id == ManagerId).First();
                    employeeDTOs.Insert(0, new EmployeeDTO
                    {
                        Id = manager.Id,
                        FirstName = manager.FirstName,
                        LastName = manager.LastName,
                        RoleName = manager.UserAccount.Role.Name,
                        RoleId = manager.UserAccount.RoleID
                    });
                }

                return employeeDTOs;
            }
            catch (Exception ex)
            {
                // Handle exceptions (log them, etc.)
                return null;
            }
        }

        public async Task<object> GetTeamInfoByEmployeeIdAsync(Guid employeeId)
        {
            try
            {
                // Find the department of the given employee
                var departmentId = await _dbContext.Employees
                    .Where(e => e.Id == employeeId)
                    .Select(e => e.DepartmentId)
                    .FirstOrDefaultAsync();

                if (departmentId == null) throw new Exception("Department not found for given employee.");

                // Fetch all employees in the department
                var employees = await _dbContext.Employees.Include(a => a.UserAccount).ThenInclude(ua => ua.Role)
                    .Where(e => e.DepartmentId == departmentId)
                    .ToListAsync();
                var managerId = _dbContext.Employees.Include(e => e.UserAccount).ThenInclude(u => u.Role).FirstOrDefault(e => e.DepartmentId == departmentId && e.UserAccount.Role.Name == "Manager") != null ? _dbContext.Employees.Include(e => e.UserAccount).ThenInclude(u => u.Role).FirstOrDefault(e => e.DepartmentId == departmentId && e.UserAccount.Role.Name == "Manager").Id : Guid.Empty;

                // Identify the manager within these employees
                var manager = employees.FirstOrDefault(e => e.UserAccount.Role.Name == "Manager");

                if (manager == null) throw new Exception("Manager not found for the department.");

                // Exclude the manager from the support team list
                var supportTeam = employees.Where(em => em.Id != manager.Id).Select(e => new EmployeeDTO
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    RoleName = e.UserAccount.Role.Name,
                    RoleId = e.UserAccount.RoleID
                    // Add other necessary mappings here
                }).ToList();

                // Construct the result including the manager at the top
                var result = new
                {
                    ManagerId = managerId,
                    ManagerName = $"{manager.FirstName} {manager.LastName}",
                    SupportHuman = supportTeam
                };

                return result; // You can directly return the anonymous object for JSON serialization
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                return new { Error = ex.Message };
            }
        }


        public async Task<Department> GetDepartmentAsync(Guid departmentId)
        {
            return _dbContext.Departments.FirstOrDefault(d => d.Id == departmentId);
        }

        public List<DepartmentDTO> GetDepartmentsWithoutManager()
        {
            // Fetch all department IDs that have a manager
            var departmentsWithManager = _dbContext.Employees
                .Where(emp => _dbContext.UserAccounts
                    .Any(ua => ua.EmployeeId == emp.Id && ua.Role.Name == "Manager"))
                .Select(emp => emp.DepartmentId)
                .Distinct()
                .ToList();

            // Fetch all departments that are NOT in the above list
            var departmentsWithoutManager = _dbContext.Departments
                .Where(dept => !departmentsWithManager.Contains(dept.Id))
                .Select(a => new DepartmentDTO
                {
                    Id = a.Id,
                    //ManagerId = a.ManagerId,
                    WorkTrackId = a.WorkTrackId,
                    Name = a.Name,
                    IsDeleted = a.IsDeleted
                }).ToList();

            return departmentsWithoutManager;
        }
    }
}
