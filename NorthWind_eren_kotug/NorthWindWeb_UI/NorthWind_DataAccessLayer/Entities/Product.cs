using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind_DataAccessLayer.Entities
{
    public class Product
    {

        public int ProductID { get; set; }
        public string? ProductName { get; set; }
        public int? SupplierID { get; set; }
        public int? CategoryID { get; set; }
        public string? QuantityPerUnit { get; set; }
        public decimal? UnitPrice { get; set; }
        public short? UnitsInStock { get; set; }
        public string? Images { get; set; }
        public bool IsActive { get; set; } 
        public virtual Category Category { get; set; }
        public DateTime? lastmodifytime { get; set; }
    }
}
