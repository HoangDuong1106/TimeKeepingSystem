using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.DTO;
using BusinessObject.Model;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class WifiRepository : Repository<Wifi>, IWifiRepository
    {
        private readonly MyDbContext _dbContext;

        public WifiRepository(MyDbContext context) : base(context)
        {
            _dbContext = context;
        }

        // Create a new Wifi record
        public async Task<object> CreateWifiAsync(WifiDTO wifi)
        {
            Wifi wifi1 = new Wifi()
            {
                Id = Guid.NewGuid(),
                name = wifi.name,
                BSSID = wifi.BSSID,
                Status = (bool)wifi.Status,
                IsDeleted = false

            };
            await _dbContext.Wifis.AddAsync(wifi1);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        // Read a Wifi record by its ID
        public async Task<Wifi> GetWifiByIdAsync(Guid id)
        {
            return await _dbContext.Wifis.FindAsync(id);
        }

        // Update an existing Wifi record
        public async Task<bool> UpdateWifiAsync(Wifi wifi)
        {
            _dbContext.Wifis.Update(wifi);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        // Soft Delete a Wifi record by its ID
        public async Task<bool> SoftDeleteWifiAsync(Guid id)
        {
            var wifi = await _dbContext.Wifis.FindAsync(id);
            if (wifi == null) return false;

            wifi.IsDeleted = true;

            return await _dbContext.SaveChangesAsync() > 0;
        }

        // Get all Wifi records
        public async Task<List<Wifi>> GetAllWifisAsync()
        {
            return await _dbContext.Wifis.ToListAsync();
        }
    }
}
