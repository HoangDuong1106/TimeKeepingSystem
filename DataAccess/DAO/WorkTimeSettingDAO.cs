using System.Text;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DataAccess.DAO
{
    public class WorkTimeSettingDAO
    {
        private static WorkTimeSettingDAO instance = null;
        private static readonly object instanceLock = new object();
        private WorkTimeSettingDAO() { }
        public static WorkTimeSettingDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new WorkTimeSettingDAO();
                    }
                    return instance;
                }
            }
        }


        public static async Task<object> GetWorkTimeSettings()
        {


            try
            {
                using (var context = new MyDbContext())
                {

                    var members = await context.WorkTimeSettings.ToListAsync();
                    return members;
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }




        public static async Task AddWorkTimeSetting(WorkTimeSetting m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var p2 = await context.WorkTimeSettings.FirstOrDefaultAsync(c => c.Id.Equals(m.Id));

                    if (p2 == null)
                    {
                        context.WorkTimeSettings.Add(m);
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
        public static async Task AddWorkDateSetting(WorkDateSetting m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var p2 = await context.WorkDateSettings.FirstOrDefaultAsync(c => c.Id.Equals(m.Id));

                    if (p2 == null)
                    {
                        context.WorkDateSettings.Add(m);
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



        public static async Task UpdateWorkTimeSetting(WorkTimeSetting m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    // Kiểm tra xem đối tượng m đã được theo dõi trong context hay chưa
                    var existingEntity = context.WorkTimeSettings.Local.FirstOrDefault(e => e.Id == m.Id);
                    if (existingEntity != null)
                    {
                        // Nếu đã được theo dõi, cập nhật trực tiếp trên đối tượng đó
                        //context.Entry(existingEntity).CurrentValues.SetValues(m);
                        context.WorkTimeSettings.Update(existingEntity);
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

        public async Task<object> GetAllWorkTimeSetting()
        {
            using (var context = new MyDbContext())
            {
                return context.WorkTimeSettings;
            }
        }

        public static async Task DeleteWorkTimeSetting(Guid p)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var member = await context.WorkTimeSettings.FirstOrDefaultAsync(c => c.Id == p);
                    if (member == null)
                    {
                        throw new Exception("Id is not Exits");
                    }
                    else
                    {
                        context.WorkTimeSettings.Remove(member);
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
