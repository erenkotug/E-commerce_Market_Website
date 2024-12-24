using NorthWind_DataAccessLayer.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NorthWind.DataAccessLayer.Entities
{
    public class ShoppingBag
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("Product")]
        public int ProductID { get; set; }
        public Product Product { get; set; }

        [ForeignKey("Member")]
        public int UsernameID { get; set; }
        public Member Member { get; set; }
        public int Quantity { get; set; }
    }
}