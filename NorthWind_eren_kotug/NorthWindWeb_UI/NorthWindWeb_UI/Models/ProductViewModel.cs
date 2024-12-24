using Microsoft.AspNetCore.Mvc;

namespace NorthWindWeb_UI.ViewModels
{
    public class ProductViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int CategoryID { get; set; }
        public decimal? UnitPrice { get; set; }
        public short? UnitsInStock { get; set; }
        public int Quantity { get; set; }
        public string? QuantityPerUnit { get; set; }
        public string? Images { get; set; }
    }

}

