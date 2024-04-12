using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { public interface IDepartmentHolidayRepository { Task<object> AddAsync(PostHolidayListDTO a); Task<List<DepartmentHolidayDTO>> GetAllAsync(); Task<bool> IsHoliday(MyDbContext dbContext, string dateString); Task<bool> SoftDeleteAsync(Guid id); } }