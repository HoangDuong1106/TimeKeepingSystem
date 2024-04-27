using System.Text;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DataAccess.DAO
{
    public class DepartmentDAO
    {
        private static DepartmentDAO instance = null;
        private static readonly object instanceLock = new object();
        private DepartmentDAO() { }
        public static DepartmentDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new DepartmentDAO();
                    }
                    return instance;
                }
            }
        }


        public static async Task<object> GetDepartments()
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    return await (from Department in context.Departments
                                  select new
                                  {
                                      Id = Department.Id,
                                      Name = Department.Name,
                                      ManagerId = context.Employees.Include(e => e.UserAccount).ThenInclude(u => u.Role).FirstOrDefault(e => e.DepartmentId == Department.Id && e.UserAccount.Role.Name == "Manager") != null ? context.Employees.Include(e => e.UserAccount).ThenInclude(u => u.Role).FirstOrDefault(e => e.DepartmentId == Department.Id && e.UserAccount.Role.Name == "Manager").Id : Guid.Empty,
                                      ManagerName = context.Employees.Include(e => e.UserAccount).ThenInclude(u => u.Role).Where(e => e.DepartmentId == Department.Id && e.UserAccount.Role.Name == "Manager").Select(employee => employee.FirstName + " " + employee.LastName).First(),
                                      workingWorkTrackId = Department.WorkTrackId,
                                      IsDeleted = Department.IsDeleted,
                                  }).ToListAsync();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static async Task AddDepartment(Team m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var p1 = await context.Departments.FirstOrDefaultAsync(c => c.Name.Equals(m.Name));
                    var p2 = await context.Departments.FirstOrDefaultAsync(c => c.Id.Equals(m.Id));
                    if (p1 == null)
                    {
                        if (p2 == null)
                        {
                            context.Departments.Add(m);
                            await context.SaveChangesAsync();
                        }
                        else
                        {
                            throw new Exception("Id is Exits");
                        }
                    }
                    else
                    {
                        throw new Exception("Email is Exist");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task UpdateDepartment(Team m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    // Kiểm tra xem đối tượng m đã được theo dõi trong context hay chưa
                    var existingEntity = context.Departments.FirstOrDefault(e => e.Id == m.Id);
                    if (existingEntity != null)
                    {
                        m.WorkTrackId = existingEntity.WorkTrackId;
                        // Nếu đã được theo dõi, cập nhật trực tiếp trên đối tượng đó
                        context.Entry(existingEntity).CurrentValues.SetValues(m);
                    }
                    else
                    {
                        // Nếu chưa được theo dõi, đính kèm và đánh dấu là sửa đổi
                        context.Attach(m);
                        context.Entry(m).State = EntityState.Modified;
                    }

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task DeleteDepartment(Guid p)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var member = await context.Departments.FirstOrDefaultAsync(c => c.Id == p);
                    if (member == null)
                    {
                        throw new Exception("Id is not Exits");
                    }
                    else
                    {
                        member.IsDeleted = true;
                        context.SaveChanges();
                    }



                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    }
}
