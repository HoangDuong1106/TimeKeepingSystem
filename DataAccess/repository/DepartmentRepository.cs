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
                var employees = await _dbContext.Employees
                    .Where(e => e.DepartmentId == departmentId)
                    .ToListAsync();

                // Convert to DTOs (assuming you have an EmployeeDTO class)
                var employeeDTOs = employees.Select(e => new EmployeeDTO
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    // ... (other fields)
                }).ToList();

                return employeeDTOs;
            }
            catch (Exception ex)
            {
                // Handle exceptions (log them, etc.)
                return null;
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
