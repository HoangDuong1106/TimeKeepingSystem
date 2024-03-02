using System.Text;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DataAccess.DAO
{
    public class HolidayDAO
    {
        private static HolidayDAO instance = null;
        private static readonly object instanceLock = new object();
        private HolidayDAO() { }
        public static HolidayDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new HolidayDAO();
                    }
                    return instance;
                }
            }
        }


        public static async Task<object> GetHolidays()
        {


            try
            {
                using (var context = new MyDbContext())
                {
                    return await (from holiday in context.DepartmentHolidays
                                  join Department in context.Departments on holiday.DepartmentId equals Department.Id

                                  group new { holiday, Department } by new { holiday.HolidayName, holiday.StartDate, holiday.EndDate } into holidayGroup
                                  select new
                                  {
                                      HolidayName = holidayGroup.Key.HolidayName,
                                      DepartmentIds = holidayGroup.Select(item => item.holiday.DepartmentId).ToList(),
                                      DepartmentNames = holidayGroup.Select(item => item.Department.Name).ToList(),
                                      StartDate = holidayGroup.Key.StartDate.ToString("yyyy/MM/dd"),
                                      EndDate = holidayGroup.Key.EndDate.ToString("yyyy/MM/dd"),

                                      Description = holidayGroup.Select(item => item.holiday.Description).FirstOrDefault(),
                                      IsRecurring = holidayGroup.Select(item => item.holiday.IsRecurring).FirstOrDefault(),
                                      IsDeleted = holidayGroup.Select(item => item.holiday.IsDeleted).FirstOrDefault(),
                                  }).ToListAsync();
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }




        public static async Task AddHoliday(DepartmentHoliday m)
        {
            try
            {
                using (var context = new MyDbContext())
                {

                    
                        context.DepartmentHolidays.Add(m);
                        await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task UpdateHoliday(DepartmentHoliday m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    // Kiểm tra xem đối tượng m đã được theo dõi trong context hay chưa
                    var existingEntity = context.DepartmentHolidays.Local.FirstOrDefault(e => e.HolidayId == m.HolidayId);
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

        public static async Task DeleteHoliday(Guid p)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var member = await context.DepartmentHolidays.FirstOrDefaultAsync(c => c.HolidayId == p);
                    if (member == null)
                    {
                        throw new Exception("Id is not Exits");
                    }
                    else
                    {
                        context.DepartmentHolidays.Remove(member);
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
