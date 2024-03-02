using System.Text;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DataAccess.DAO
{
    public class LeaveSettingDAO
    {
        private static LeaveSettingDAO instance = null;
        private static readonly object instanceLock = new object();
        private LeaveSettingDAO() { }
        public static LeaveSettingDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new LeaveSettingDAO();
                    }
                    return instance;
                }
            }
        }


        public static async Task<List<LeaveSetting>> GetLeaveSettings()
        {


            try
            {
                using (var context = new MyDbContext())
                {

                    var members = await context.LeaveSettings.ToListAsync();
                    return members;
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }




        public static async Task AddLeaveSetting(LeaveSetting m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var p2 = await context.LeaveSettings.FirstOrDefaultAsync(c => c.LeaveSettingId.Equals(m.LeaveSettingId));

                    if (p2 == null)
                    {
                        context.LeaveSettings.Add(m);
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

        public static async Task UpdateLeaveSetting(LeaveSetting m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    // Kiểm tra xem đối tượng m đã được theo dõi trong context hay chưa
                    var existingEntity = context.LeaveSettings.Local.FirstOrDefault(e => e.LeaveSettingId == m.LeaveSettingId);
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

        public static async Task DeleteLeaveSetting(Guid p)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var member = await context.LeaveSettings.FirstOrDefaultAsync(c => c.LeaveSettingId == p);
                    if (member == null)
                    {
                        throw new Exception("Id is not Exits");
                    }
                    else
                    {
                        context.LeaveSettings.Remove(member);
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
