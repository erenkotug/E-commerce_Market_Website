using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind_DataAccessLayer.Entities
{
    public class MemberOrder
    {
        [Key]
        public int OrderID { get; set; }

        [ForeignKey("Member")]
        public int MemberID { get; set; }

        [ForeignKey("Shipper")]
        public int ShipperID { get; set; }

        [ForeignKey("Address")]
        public int AddressID { get; set; }

        [Required]
        public DateTime DeliveryDate { get; set; }
        public string? OrderFee { get; set; }
        public DateTime OrderCreated { get; set; }


    }

}
