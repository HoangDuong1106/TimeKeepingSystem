using BusinessObject.DTO;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DataAccess.Repository
{
    public class WorkDateSettingRepository : Repository<WorkDateSetting>, IWorkDateSettingRepository
    {
        private readonly MyDbContext _dbContext;

        public WorkDateSettingRepository(MyDbContext context) : base(context)
        {
            // You can add more specific methods here if needed
            _dbContext = context;
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

        public async Task<List<WorkDateSettingDTO>> GetAll()
        {
            var results = new List<WorkDateSettingDTO>();
            _dbContext.WorkDateSettings.ToList().ForEach(result =>
            {
                DateStatusDTO? dateStatusDTO = JsonSerializer.Deserialize<DateStatusDTO>(result.DateStatus) ?? null;
                results.Add(new WorkDateSettingDTO
                {
                    Id = result.Id,
                    DateStatus = dateStatusDTO,
                    IsDeleted = result.IsDeleted
                });
            });

            return results;
        }

        public async Task<object> Create(WorkDateSettingDTO dto)
        {
            string dateStatusJson = dto.DateStatus != null ? JsonSerializer.Serialize<DateStatusDTO>(dto.DateStatus) : null;

            WorkDateSetting entity = new WorkDateSetting
            {
                Id = Guid.NewGuid(),
                DateStatus = dateStatusJson,
                IsDeleted = false
            };

            _dbContext.WorkDateSettings.Add(entity);
            await _dbContext.SaveChangesAsync();

            return new
            {
                WorkDateSettingId = entity.Id
            };
        }

        // Read
        public async Task<WorkDateSettingDTO> GetById(Guid id)
        {
            var result = _dbContext.WorkDateSettings.Where(w => w.Id == id).FirstOrDefault();
            if (result == null) throw new Exception("WorkDateSetting not existing");
            return new WorkDateSettingDTO
            {
                Id = result.Id,
                DateStatus = JsonSerializer.Deserialize<DateStatusDTO>(result.DateStatus) ?? null,
                IsDeleted= result.IsDeleted
            };
        }

        // Update
        public async Task<bool> Update(WorkDateSettingDTO dto)
        {
            WorkDateSetting entity = await _dbContext.WorkDateSettings.FindAsync(dto.Id);
            if (entity == null)
            {
                throw new Exception("WorkDateSetting not existing");
            }

            if (dto.DateStatus != null)
            {
                entity.DateStatus = JsonSerializer.Serialize(dto.DateStatus);
            }

            if (dto.IsDeleted != null)
            {
                entity.IsDeleted = (bool)dto.IsDeleted;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Delete (Soft Delete)
        public async Task<bool> Delete(Guid id)
        {
            WorkDateSetting entity = await _dbContext.WorkDateSettings.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = true;

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
