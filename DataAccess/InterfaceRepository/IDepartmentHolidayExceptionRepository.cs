using BusinessObject.DTO;

namespace DataAccess.InterfaceRepository { 
    public interface IDepartmentHolidayExceptionRepository 
    { 
        Task<bool> AddAsync(DepartmentHolidayExceptionDTO a); 
        Task<List<DepartmentHolidayExceptionDTO>> GetAllAsync(); 
        Task<bool> SoftDeleteAsync(Guid id); 
    } 
}