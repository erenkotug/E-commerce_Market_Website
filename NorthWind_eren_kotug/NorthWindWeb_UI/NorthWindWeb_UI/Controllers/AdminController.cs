using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NorthWind_DataAccessLayer.Data;
using NorthWind_DataAccessLayer.Entities;
using NorthWindWeb_UI.ViewModels;
using System.Drawing.Printing;
using System.Text;
using static NorthWindWeb_UI.Controllers.AccountController;

namespace NorthWindWeb_UI.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 20;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Home()
        {
            return View();
        }
        public IActionResult CommonDashboard()
        {

            var topProducts = _context.MemberOrderProducts
                .GroupBy(m => m.ProductID)
                .Select(g => new
                {
                    ProductID = g.Key,
                    Quantity = g.Sum(m => m.Quantity)
                })
                .OrderByDescending(g => g.Quantity)
                .Take(10)
                .ToList();

            var productIds = topProducts.Select(p => p.ProductID).ToList();
            var products = _context.Products
                .Where(p => productIds.Contains(p.ProductID))
                .ToDictionary(p => p.ProductID, p => p.ProductName);

            var chartData = topProducts.Select(p => new
            {
                ProductName = products[p.ProductID],
                Quantity = p.Quantity
            }).ToList();

            ViewBag.ChartData = chartData;

            var categorySales = (from p in _context.Products
                                 join c in _context.Categories on p.CategoryID equals c.CategoryID
                                 join op in _context.MemberOrderProducts on p.ProductID equals op.ProductID
                                 group new { c.CategoryName, op.Quantity } by c.CategoryName into g
                                 select new
                                 {
                                     CategoryName = g.Key,
                                     TotalQuantitySold = g.Sum(x => x.Quantity)
                                 }).ToList();

            ViewBag.CategorySales = categorySales;

            var memberOrders = _context.MemberOrders
            .ToList(); // Verileri belleğe al

            // Verileri gruplandır ve işleme al
            var userOrderStats = memberOrders
                .GroupBy(mo => mo.MemberID)
                .Select(g => new
                {
                    MemberID = g.Key,
                    OrderCount = g.Count(),
                    TotalOrderFee = g.Sum(mo =>
                    {
                        decimal fee;
                        return decimal.TryParse(mo.OrderFee, out fee) ? fee : 0;
                    })
                })
                .OrderByDescending(g => g.TotalOrderFee)
                .Take(5)
                .ToList();

            // Kullanıcı adlarını almak için Member tablosunu join et
            var topMembers = (from uos in userOrderStats
                              join m in _context.Members on uos.MemberID equals m.MemberID
                              select new
                              {
                                  Username = m.Username,
                                  OrderCount = uos.OrderCount,
                                  TotalOrderFee = (uos.TotalOrderFee / 100) // 100'e bölüp iki ondalıklı basamakla biçimlendir
                              }).ToList();

            // ViewBag'e verileri ata
            ViewBag.TopMembers = topMembers;

            var startDate = new DateTime(2020, 1, 1);
            var endDate = new DateTime(2025, 12, 31);

            // Veriyi belleğe al
            var memberOrders_ = _context.MemberOrders
                .Where(mo => mo.OrderCreated >= startDate && mo.OrderCreated <= endDate)
                .ToList();

            var periods = memberOrders_
                .Select(mo =>
                {
                    // 6 aylık dönemi hesapla
                    var orderDate = mo.OrderCreated;
                    int periodStartMonth = ((orderDate.Month - 1) / 6) * 6 + 1;
                    int periodEndMonth = periodStartMonth + 5;

                    // Dönem başlangıcı ve bitişi tarihlerini oluştur
                    var periodStart = new DateTime(orderDate.Year, periodStartMonth, 1);
                    var periodEnd = periodStart.AddMonths(6).AddDays(-1);

                    // OrderFee'yi decimal'e dönüştür
                    return new
                    {
                        PeriodStart = periodStart,
                        PeriodEnd = periodEnd,
                        OrderFee = decimal.TryParse(mo.OrderFee, out decimal fee) ? fee : 0
                    };
                })
                .GroupBy(x => new { x.PeriodStart, x.PeriodEnd })
                .Select(g => new
                {
                    PeriodStart = g.Key.PeriodStart,
                    PeriodEnd = g.Key.PeriodEnd,
                    TotalOrderFee = g.Sum(x => x.OrderFee / 100)
                })
                .OrderBy(p => p.PeriodStart)
                .ToList();

            ViewBag.Periods = periods;

            var shipperPrices = _context.Shippers
                .ToDictionary(s => s.ShipperID, s => decimal.TryParse(s.ShipperPrice.Replace(',', '.'), out var price) ? price : 0);


            var shipperData = memberOrders
                .GroupBy(mo => mo.ShipperID)
                .Select(g => new
                {
                    ShipperID = g.Key,
                    ShipperCount = g.Count(),
                    TotalPrice = g.Count() * (shipperPrices.ContainsKey(g.Key) ? shipperPrices[g.Key] : 0)
                })
                .Join(_context.Shippers, mo => mo.ShipperID, s => s.ShipperID, (mo, s) => new
                {
                    CompanyName = s.CompanyName,
                    ShipperCount = mo.ShipperCount,
                    TotalPrice = mo.TotalPrice / 100
                })
                .OrderByDescending(result => result.ShipperCount)
                .ToList();

            ViewBag.ShipperData = shipperData;

            var topCities = _context.MemberOrders
                .Join(
                    _context.Addresses,
                    mo => mo.AddressID,
                    a => a.AddressID,
                    (mo, a) => new { a.City }
                )
                .GroupBy(a => a.City)
                .Select(g => new
                {
                    City = g.Key,
                    CityCount = g.Count()
                })
                .OrderByDescending(x => x.CityCount)
                .Take(5)
                .ToList();

            ViewBag.TopCities = topCities;

            return View();
        }

        #region User Management
        public IActionResult UserManagement(int page = 1)
        {
            var totalMembers = _context.Members.Count();
            var totalPages = (int)Math.Ceiling(totalMembers / (double)PageSize);

            var members = _context.Members
                .OrderBy(m => m.Username) // Sıralama yapabilirsiniz
            .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.Members = members;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View();
        }
        [HttpPost]
        public IActionResult Edit(Member model)
        {
            if (model.Name != null && model.Surname != null && model.Email != null)
            {
                var existingMember = _context.Members.FirstOrDefault(m => m.Username == model.Username);
                if (existingMember != null)
                {
                    existingMember.Name = model.Name;
                    existingMember.Surname = model.Surname;
                    existingMember.Email = model.Email;
                    existingMember.lastmodifytime = DateTime.Now;

                    _context.Members.Update(existingMember);
                    _context.SaveChanges();

                    return Json(new { success = true });
                }
            }
            return Json(new { success = false, message = "Invalid data or member not found." });
        }
        public IActionResult GetMemberByUsername(string username)
        {
            var member = _context.Members.FirstOrDefault(m => m.Username == username);
            if (member == null)
            {
                return Json(null); 
            }
            return Json(new { username = member.Username, name = member.Name, surname = member.Surname, email = member.Email });
        }
        [HttpPost]
        public IActionResult ToggleActiveStatus(string username, int IsActive)
        {
            var member = _context.Members.FirstOrDefault(m => m.Username == username);
            if (member == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            member.IsActive = IsActive == 1 ? 0 : 1;
            member.lastmodifytime = DateTime.Now;

            _context.SaveChanges();

            return Json(new { success = true });
        }


        #endregion

        #region Product Management

        public IActionResult ProductManagement(int page = 1)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Select(p => new
                {
                    p.ProductID,
                    p.ProductName,
                    CategoryName = p.Category.CategoryName,
                    p.UnitPrice,
                    p.UnitsInStock,
                    p.IsActive // Convert int to bool if necessary
                })
                .ToList();

            var categories = _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryID.ToString(),
                    Text = c.CategoryName
                })
                .ToList();

            ViewBag.Categories = categories;

            int pageSize = 14;
            int totalItems = products.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.Products = pagedProducts;

            return View();
        }

        public IActionResult GetProductById(int id)
        {
            var product = _context.Products
                .Include(p => p.Category) // Ensure navigation property is used
                .Where(p => p.ProductID == id)
                .Select(p => new
                {
                    p.ProductID,
                    p.ProductName,
                    CategoryID = p.Category.CategoryID, // Use CategoryID for dropdown
                    p.UnitPrice,
                    p.UnitsInStock,
                    p.IsActive // Include IsActive in the projection
                })
                .FirstOrDefault();

            return Json(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = _context.Products.Find(model.ProductID);

                if (product != null)
                {
                    product.ProductName = model.ProductName;
                    product.UnitPrice = model.UnitPrice;
                    product.UnitsInStock = model.UnitsInStock;
                    product.CategoryID = model.CategoryID;
                    product.lastmodifytime = DateTime.Now;

                    if (!string.IsNullOrEmpty(model.Images)) // Update existing image
                    {
                        product.Images = model.Images;
                    }

                    _context.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Product not found." });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "Invalid data.", errors = errors });
        }

        [HttpPost]
        public IActionResult ToggleActiveStatusProduct(int productId, bool isActive)
        {
            var product = _context.Products.FirstOrDefault(m => m.ProductID == productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }
            if (product.IsActive == false)
            {
                product.IsActive = true;
            }
            else if (product.IsActive == true)
            {
                product.IsActive = false;
            }
            product.lastmodifytime = DateTime.Now;

            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductViewModel model, IFormFile addImage)
        {
            if (model.ProductName != null &&
                model.UnitPrice != null &&
                model.QuantityPerUnit != null &&
                model.CategoryID != 0)
            {
                var imageHexString = string.Empty;

                if (addImage != null && addImage.Length > 0)
                {
                    var fileBytes = FileHelper.ReadFileBytes(addImage);
                    imageHexString = FileHelper.ConvertToHex(fileBytes);
                }

                var product = new Product
                {
                    ProductName = model.ProductName,
                    CategoryID = model.CategoryID,
                    QuantityPerUnit = model.QuantityPerUnit,
                    UnitPrice = model.UnitPrice,
                    UnitsInStock = model.UnitsInStock,
                    Images = imageHexString,
                    IsActive = true,
                    lastmodifytime = DateTime.Now
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, errors });
        }

        public static class FileHelper
        {
            public static string ConvertToHex(byte[] bytes)
            {
                StringBuilder hex = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes)
                {
                    hex.AppendFormat("{0:X2}", b);
                }
                return hex.ToString();
            }

            public static byte[] ReadFileBytes(IFormFile file)
            {
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        private async Task<byte[]> ReadFileAsByteArray(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static string ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        #endregion

        #region Category Management
        public IActionResult CategoryManagement(int page = 1)
        {
            
                var categories = _context.Categories
                    .Select(p => new
                    {
                        p.CategoryID,
                        p.CategoryName,
                        p.Description,
                    })
                    .ToList();

                int pageSize = 14;
                int totalItems = categories.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                var pagedCategories = categories.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                ViewBag.Categories = pagedCategories;

                return View();
        }
        public IActionResult GetCategoryById(int id)
        {
            var category = _context.Categories
                .Where(c => c.CategoryID == id)
                .Select(c => new
                {
                    c.CategoryID,
                    c.CategoryName,
                    c.Description
                })
                .FirstOrDefault();

            if (category == null)
            {
                return Json(null);
            }

            return Json(category);
        }
        [HttpPost]
        public IActionResult EditCategory(int CategoryID, string CategoryName, string Description)
        {
            var category = _context.Categories.Find(CategoryID);

            if (category != null)
            {
                category.CategoryName = CategoryName;
                category.Description = Description;

                _context.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Category not found." });
        }

        [HttpPost]
        public JsonResult AddCategory(string CategoryName, string Description)
        {
            try
            {
                if (string.IsNullOrEmpty(CategoryName) || string.IsNullOrEmpty(Description))
                {
                    return Json(new { success = false, message = "Category Name and Description are required." });
                }

                var newCategory = new Category
                {
                    CategoryName = CategoryName,
                    Description = Description
                };

                _context.Categories.Add(newCategory);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while adding the category: " + ex.Message });
            }
        }
        #endregion
    }
}
