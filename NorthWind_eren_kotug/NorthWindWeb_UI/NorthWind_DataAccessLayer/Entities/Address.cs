using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NorthWind_DataAccessLayer.Entities
{
    public class Address
    {
        public int AddressID { get; set; }

        [Required]
        public string Street { get; set; }

        public string Apt { get; set; }

        [Required]
        public string Province { get; set; }

        [Required]
        public string City { get; set; }

        public string Neighborhood { get; set; }

        [Required]
        public string Postcode { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string Phone { get; set; }

        public int MemberID { get; set; }
    }

}
