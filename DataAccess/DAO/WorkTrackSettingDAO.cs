using System.Text;
using BusinessObject.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Data;
using BusinessObject.DTO;
using System.Text.Json;

namespace DataAccess.DAO
{
    public class WorkTrackSettingDAO
    {
        private static WorkTrackSettingDAO instance = null;
        private static readonly object instanceLock = new object();
        private WorkTrackSettingDAO() { }
        public static WorkTrackSettingDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new WorkTrackSettingDAO();
                    }
                    return instance;
                }
            }
        }


        public static async Task<object> GetWorkTrackSettings()
        {
            try
            {
                using (var context = new MyDbContext())
                {

                    var members = await context.WorkTrackSettings.ToListAsync();
                    return members;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public static async Task<object> GetWorkDateSettingByDeparment(Guid Id)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var results = await (from department in context.Departments
                                         where department.Id.Equals(Id)
                                         join workTrack in context.WorkTrackSettings on department.WorkTrackId equals workTrack.Id
                                         join workDate in context.WorkDateSettings on workTrack.WorkDateId equals workDate.Id
                                         select new
                                         {
                                             id = workDate.Id,
                                             DateStatus = workDate.DateStatus,
                                             isDeleted = workDate.IsDeleted
                                         }).FirstOrDefaultAsync();


                    return new
                    {
                        Id = results.id,
                        DateStatus = JsonSerializer.Deserialize<DateStatusDTO>(results.DateStatus) ?? null,
                        IsDeleted = results.isDeleted
                    };


                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }
        public static async Task<object> GetLeaveSettingByDeparment(Guid Id)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    return await (from department in context.Departments
                                  where department.Id.Equals(Id)
                                  join workTrack in context.WorkTrackSettings on department.WorkTrackId equals workTrack.Id
                                  join leaveTrack in context.LeaveSettings on workTrack.LeaveSettingId equals leaveTrack.LeaveSettingId
                                  select new
        {
            LeaveSettingId = leaveTrack.LeaveSettingId,
                                      MaxDateLeave = leaveTrack.MaxDateLeave,
            IsManagerAssigned = leaveTrack.IsManagerAssigned,
            IsDeleted = leaveTrack.IsDeleted
                                  }).FirstOrDefaultAsync();

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }
        public static async Task<object> GetWorkTimeSettingByDeparment(Guid Id)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    return await (from department in context.Departments
                                  where department.Id.Equals(Id)
                                  join workTrack in context.WorkTrackSettings on department.WorkTrackId equals workTrack.Id
                                  join workTime in context.WorkTimeSettings on workTrack.WorkTimeId equals workTime.Id
                                  select new
                                  {
                                      id = workTime.Id,
                                      FromHourMorning = workTime.FromHourMorning,
                                      ToHourMorning = workTime.ToHourMorning,
                                      FromHourAfternoon = workTime.FromHourAfternoon,
                                      ToHourAfternoon = workTime.ToHourAfternoon,
                                  }).FirstOrDefaultAsync();

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }
        public static async Task<object> GetRiskSettingByDeparment(Guid Id)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    return await (from department in context.Departments
                                  where department.Id.Equals(Id)
                                  join workTrack in context.WorkTrackSettings on department.WorkTrackId equals workTrack.Id
                                  join risk in context.RiskPerformanceSettings on workTrack.RiskPerfomanceId equals risk.Id

                                  select new
                                  {
                                      id = risk.Id,
                                      Hours = risk.Hours,
                                      Days = risk.Days,
                                      DateSet = risk.DateSet,
                                  }).FirstOrDefaultAsync();


                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static async Task AddWorkTrackSetting(WorkTrackSetting m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var p2 = await context.WorkTrackSettings.FirstOrDefaultAsync(c => c.Id.Equals(m.Id));

                    if (p2 == null)
                    {
                        context.WorkTrackSettings.Add(m);
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

        public static async Task UpdateWorkTrackSetting(WorkTrackSetting m)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    // Kiểm tra xem đối tượng m đã được theo dõi trong context hay chưa
                    var existingEntity = context.WorkTrackSettings.Local.FirstOrDefault(e => e.Id == m.Id);
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

        public static async Task DeleteWorkTrackSetting(Guid p)
        {
            try
            {
                using (var context = new MyDbContext())
                {
                    var member = await context.WorkTrackSettings.FirstOrDefaultAsync(c => c.Id == p);
                    if (member == null)
                    {
                        throw new Exception("Id is not Exits");
                    }
                    else
                    {
                        context.WorkTrackSettings.Remove(member);
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
