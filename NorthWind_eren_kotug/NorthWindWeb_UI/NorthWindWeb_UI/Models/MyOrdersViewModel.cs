using Microsoft.AspNetCore.Mvc;
using NorthWind_DataAccessLayer.Entities;

namespace NorthWindWeb_UI.Models
{
    public class MyOrdersViewModel
    {
        public List<MemberOrder> MemberOrders { get; set; }
        public List<MemberOrderProduct> MemberOrderProducts { get; set; }
        public List<Address> Addresses { get; set; }
        public List<Product> Products { get; set; }
        public List<Shipper> Shippers { get; set; }

    }
}
