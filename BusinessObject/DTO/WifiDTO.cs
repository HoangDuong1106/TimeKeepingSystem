using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO
{
    public class WifiDTO
    {
        public Guid? Id { get; set; }
        public string? name { get; set; }
        public string? BSSID { get; set; }
        public bool? Status { get; set; }
    }
}
