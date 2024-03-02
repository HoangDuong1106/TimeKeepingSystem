using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.DTO;
using BusinessObject.Model;
using DataAccess.InterfaceService;
using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WifiController : ControllerBase
    {
        private readonly IWifiService _wifiService;

        public WifiController(IWifiService wifiService)
        {
            _wifiService = wifiService;
        }

        // POST: api/Wifi
        [HttpPost]
        public async Task<ActionResult<object>> CreateWifi([FromBody] WifiDTO wifi)
        {
            return await _wifiService.CreateWifiAsync(wifi);
        }

        // GET: api/Wifi
        [HttpGet]
        public async Task<ActionResult<List<Wifi>>> GetAllWifis()
        {
            return await _wifiService.GetAllWifisAsync();
        }

        // GET: api/Wifi/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Wifi>> GetWifiById(Guid id)
        {
            return await _wifiService.GetWifiByIdAsync(id);
        }

        // PUT: api/Wifi
        [HttpPut]
        public async Task<ActionResult<bool>> UpdateWifi([FromBody] Wifi wifi)
        {
            return await _wifiService.UpdateWifiAsync(wifi);
        }

        // DELETE: api/Wifi/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteWifi(Guid id)
        {
            return await _wifiService.SoftDeleteWifiAsync(id);
        }
    }
}
