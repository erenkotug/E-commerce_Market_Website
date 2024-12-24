using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind_DataAccessLayer.Entities
{
    public class Shipper
    {
        public int ShipperID { get; set; }
        public string? CompanyName { get; set; }
        public string? ShipperPrice { get; set; } 
        public int FirstDate { get; set; } 
        public int LastDate { get; set; }
        public string? Note { get; set; }
    }
}
