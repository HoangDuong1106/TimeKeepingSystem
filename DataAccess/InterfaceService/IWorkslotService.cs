using BusinessObject.DTO;
using System;

namespace DataAccess.InterfaceService
{
    public interface IWorkslotService
    {
        // Empty interface
        Task<List<Workslot>> GenerateWorkSlotsForMonth(CreateWorkSlotRequest request);
        Task<List<object>> GetWorkSlotsForDepartment(CreateWorkSlotRequest request);
    }
}
