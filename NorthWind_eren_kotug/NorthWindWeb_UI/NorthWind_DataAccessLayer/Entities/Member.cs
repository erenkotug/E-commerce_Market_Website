using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind_DataAccessLayer.Entities
{
    public class Member
    {
        public int MemberID { get; set; }
        public string Username { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? UserImage { get; set; }
        public int? IsActive { get; set; }
        public DateTime? lastmodifytime { get; set; }

    }
}
