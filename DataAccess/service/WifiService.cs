using BusinessObject.DTO;
using BusinessObject.Model;
using DataAccess.InterfaceRepository;
using DataAccess.InterfaceService;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.service
{
    public class WifiService : IWifiService
    {
        private readonly IWifiRepository _wifiRepository;
        public WifiService(IWifiRepository wifiRepository) {
            _wifiRepository = wifiRepository;
        }
        public async Task<object> CreateWifiAsync(WifiDTO wifi)
        {
            return await _wifiRepository.CreateWifiAsync(wifi);
        }

        // Read a Wifi record by its ID
        public async Task<Wifi> GetWifiByIdAsync(Guid id)
        {
            return await _wifiRepository.GetWifiByIdAsync(id);
        }

        // Update an existing Wifi record
        public async Task<bool> UpdateWifiAsync(Wifi wifi)
        {
            return await _wifiRepository.UpdateWifiAsync(wifi);
        }

        // Soft Delete a Wifi record by its ID
        public async Task<bool> SoftDeleteWifiAsync(Guid id)
        {
            return await _wifiRepository.SoftDeleteWifiAsync(id);
        }

        // Get all Wifi records
        public async Task<List<Wifi>> GetAllWifisAsync()
        {
            return await _wifiRepository.GetAllWifisAsync();
        }
    }
}
