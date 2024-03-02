using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { public interface IDepartmentHolidayRepository { Task<bool> AddAsync(DepartmentHolidayDTO a); Task<List<DepartmentHolidayDTO>> GetAllAsync(); Task<bool> SoftDeleteAsync(Guid id); } }