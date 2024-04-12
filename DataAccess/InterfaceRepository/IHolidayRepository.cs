using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { public interface IHolidayRepository { Task<object> AddAsync(PostHolidayListDTO a); Task<List<DepartmentHolidayDTO>> GetAllAsync(); Task<bool> SoftDelete(Guid[] holidayIds); } }