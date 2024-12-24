using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthWind_DataAccessLayer.Data;
using NorthWind_DataAccessLayer.Entities;

namespace NorthWindWeb_UI.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Default(string category, string priceRange, string sort, string stock)
        {
            IEnumerable<Product> products = _context.Products
                                                    .Where(p => p.IsActive == true)
                                                    .ToList();

            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.CategoryID.ToString() == category);
            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                var sanitizedPriceRange = priceRange.Replace("$", "").Replace(" ", "");
                var priceRangeParts = sanitizedPriceRange.Split('-');
                if (priceRangeParts.Length == 2 &&
                    double.TryParse(priceRangeParts[0], out double minPrice) &&
                    double.TryParse(priceRangeParts[1], out double maxPrice))
                {
                    products = products.Where(p => (double)p.UnitPrice >= minPrice && (double)p.UnitPrice <= maxPrice);
                }
                else if (double.TryParse(priceRangeParts[0], out minPrice))
                {
                    products = products.Where(p => (double)p.UnitPrice >= minPrice);
                }
            }

            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "High-Low":
                        products = products.OrderByDescending(p => p.UnitPrice);
                        break;
                    case "Low-High":
                        products = products.OrderBy(p => p.UnitPrice);
                        break;
                }
            }
            if (!string.IsNullOrEmpty(stock))
            {
                switch (stock)
                {
                    case "In":
                        products = products.Where(p => p.UnitsInStock != 0);
                        break;
                    case "Out":
                        products = products.Where(p => p.UnitsInStock == 0);
                        break;
                }
            }

            var categories = _context.Categories.ToList();
            ViewBag.Categories = categories;

            return View(products);
        }
    }
    
}
