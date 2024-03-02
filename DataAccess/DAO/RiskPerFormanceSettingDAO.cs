using System.Text;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DataAccess.DAO
{
    public class RiskPerformanceSettingDAO
    {
        private static RiskPerformanceSettingDAO instance = null;
        private static readonly object instanceLock = new object();
        private RiskPerformanceSettingDAO() { }
        public static RiskPerformanceSettingDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new RiskPerformanceSettingDAO();
                    }
                    return instance;
                }
            }
        }


        public static async Task<object> GetRiskPerformanceSettings()
        {


            try
            {
                using (var context = new MyDbContext())
                {

                    var members = await context.RiskPerformanceSettings.ToListAsync();
                    return members;
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }




        public static async Task AddRiskPerformanceSetting(RiskPerformanceSetting m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var p2 = await context.RiskPerformanceSettings.FirstOrDefaultAsync(c => c.Id.Equals(m.Id));

                    if (p2 == null)
                    {
                        context.RiskPerformanceSettings.Add(m);
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        throw new Exception("Id is Exits");
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task UpdateRiskPerformanceSetting(RiskPerformanceSetting m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    // Kiểm tra xem đối tượng m đã được theo dõi trong context hay chưa
                    var existingEntity = context.RiskPerformanceSettings.Local.FirstOrDefault(e => e.Id == m.Id);
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

        public static async Task DeleteRiskPerformanceSetting(Guid p)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var member = await context.RiskPerformanceSettings.FirstOrDefaultAsync(c => c.Id == p);
                    if (member == null)
                    {
                        throw new Exception("Id is not Exits");
                    }
                    else
                    {
                        context.RiskPerformanceSettings.Remove(member);
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
