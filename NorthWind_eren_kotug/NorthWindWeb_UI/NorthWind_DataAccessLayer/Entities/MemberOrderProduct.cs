using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind_DataAccessLayer.Entities
{
    public class MemberOrderProduct
    {
        [Key]
        public int OrderProductID { get; set; }

        [ForeignKey("MemberOrder")]
        public int OrderID { get; set; }

        [ForeignKey("Product")]
        public int ProductID { get; set; }
        public int Quantity { get; set; }

    }

}
