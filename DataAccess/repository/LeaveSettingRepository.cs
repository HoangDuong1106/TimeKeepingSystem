using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DataAccess.Repository
{
    public class LeaveSettingRepository : Repository<LeaveSetting>, ILeaveSettingRepository
    {
        private readonly MyDbContext _dbContext;

        public LeaveSettingRepository(MyDbContext context) : base(context)
        {
            // You can add more specific methods here if needed
            _dbContext = context;
        }

        public async Task<List<LeaveSettingDTO>> GetAllAsync()
        {
            var ass = await base.GetAllAsync();
            var result = new List<LeaveSettingDTO>();
            ass.ToList().ForEach(a => result.Add(new LeaveSettingDTO
            {
                LeaveSettingId = a.LeaveSettingId,
                MaxDateLeaves = JsonSerializer.Deserialize<List<MaxDateLeaveDTO>>(a.MaxDateLeave),
                IsManagerAssigned = a.IsManagerAssigned,
                IsDeleted = a.IsDeleted
            }));

            return result;
        }

        public async Task<bool> AddAsync(LeaveSettingDTO a)
        {
            try
            {
                await base.AddAsync(new LeaveSetting() // have dbSaveChange inside method
                {
                    LeaveSettingId = (Guid)a.LeaveSettingId,
                    MaxDateLeave = JsonSerializer.Serialize(a.MaxDateLeaves),
                    IsManagerAssigned = (bool)a.IsManagerAssigned,
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

        public async Task<object> UpdateLeaveSettingAsync(LeaveSettingDTO updatedLeaveSetting)
        {
            try
            {
                var existingLeaveSetting = _dbContext.LeaveSettings.FirstOrDefault(l => l.LeaveSettingId == updatedLeaveSetting.LeaveSettingId);

                if (existingLeaveSetting == null)
                {
                    // LeaveSetting with the provided ID does not exist.
                    return false;
                }

                // Update the properties of the existing LeaveSetting entity.
                if (existingLeaveSetting.IsManagerAssigned != null)
                {
                    existingLeaveSetting.IsManagerAssigned = (bool)updatedLeaveSetting.IsManagerAssigned;
                }
                if (existingLeaveSetting.MaxDateLeave != null)
                {
                    existingLeaveSetting.MaxDateLeave = JsonSerializer.Serialize(updatedLeaveSetting.MaxDateLeaves);
                }
                if (existingLeaveSetting.IsDeleted != null)
                {
                    existingLeaveSetting.IsDeleted = (bool)updatedLeaveSetting.IsDeleted;
                }

                // Call a method that saves the changes to the database.
                await _dbContext.SaveChangesAsync();

                return new
                {
                    isSuccess = true
                };
            }
            catch (Exception ex)
            {
                // Handle exceptions here, e.g., log the error.
                return new
                {
                    isSuccess = false,
                    error = ex.Message
                };
            }
        }

    }
}
