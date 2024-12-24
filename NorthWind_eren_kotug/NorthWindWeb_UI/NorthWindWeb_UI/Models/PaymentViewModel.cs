using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using NorthWind_DataAccessLayer.Entities;
using NorthWind.DataAccessLayer.Entities;
using static NorthWindWeb_UI.Controllers.PaymentController;

namespace NorthWindWeb_UI.Models
{
    public class PaymentViewModel
    {
        public List<Payment> Payments { get; set; }
        public Payment NewPayment { get; set; }
        public List<Address> Addresses { get; set; }
        public List<ShoppingBagItemViewModel> ShoppingBags { get; set; }
        public List<Payment> SavedPayments { get; set; }
        public List<Shipper> SavedShippers { get; set; }
        public decimal? TotalSum { get; set; } 
    }

}
