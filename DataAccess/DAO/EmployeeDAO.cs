using System.Text;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DataAccess.DAO
{
    public class EmployeeDAO
    {
        private static EmployeeDAO instance = null;
        private static readonly object instanceLock = new object();
        private EmployeeDAO() { }
        public static EmployeeDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new EmployeeDAO();
                    }
                    return instance;
                }
            }
        }


        public static async Task<List<Employee>> GetEmployees()
        {
            var members = new List<Employee>();

            try
            {
                using (var context = new MyDbContext())
                {
                    members = await context.Employees.ToListAsync();

                }
                return members;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }
       


     
        public static async Task AddEmployee(Employee m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var p1 = await context.Employees.FirstOrDefaultAsync(c => c.Email.Equals(m.Email));

                    var p2 = await context.Employees.FirstOrDefaultAsync(c => c.Id.Equals(m.Id));
                    if (p1 == null)
                    {
                        if (p2 == null)
                        {
                            context.Employees.Add(m);
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

        public static async Task UpdateEmployee(Employee m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    // Kiểm tra xem đối tượng m đã được theo dõi trong context hay chưa
                    var existingEntity = context.Employees.Local.FirstOrDefault(e => e.Id == m.Id);
                    if (existingEntity != null)
                    {
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

        public static async Task DeleteEmployee(Guid p)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var member = await context.Employees.FirstOrDefaultAsync(c => c.Id == p);
                    if (member == null)
                    {
                        throw new Exception("Id is not Exits");
                    }
                    else
                    {
                        context.Employees.Remove(member);
                        await context.SaveChangesAsync();
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
