using BusinessObject.DTO;
using BusinessObject.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.InterfaceService
{
    public interface IWifiService
    {
        Task<object> CreateWifiAsync(WifiDTO wifi);
        Task<List<Wifi>> GetAllWifisAsync();
        Task<Wifi> GetWifiByIdAsync(Guid id);
        Task<bool> SoftDeleteWifiAsync(Guid id);
        Task<bool> UpdateWifiAsync(Wifi wifi);
    }
}
