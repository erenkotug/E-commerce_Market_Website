using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthWind_DataAccessLayer.Data;
using NorthWind_DataAccessLayer.Entities;
using NorthWindWeb_UI.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace NorthWindWeb_UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Login

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
            return new NoContentResult();
        }
        public IActionResult Login()
        {
            var viewModel = new LoginRegisterViewModel
            {
                LoginViewModel = new LoginViewModel(),
                RegisterViewModel = new RegisterViewModel()
            };
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginRegisterViewModel model)
        {
            if (!string.IsNullOrEmpty(model.LoginViewModel?.Username) &&
                !string.IsNullOrEmpty(model.LoginViewModel?.Password))
            {
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Username == model.LoginViewModel.Username && a.Password == model.LoginViewModel.Password);
                var member = await _context.Members
                    .FirstOrDefaultAsync(a => a.Username == model.LoginViewModel.Username && a.Password == model.LoginViewModel.Password);

                if (admin != null)
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, admin.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    return RedirectToAction("CommonDashboard", "Admin");
                }

                if (member != null)
                {
                    if (member.IsActive != 1)
                    {
                        ModelState.AddModelError("", "Your account is not active. Please contact support.");
                        return View(model);
                    }

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, member.Username),
                new Claim(ClaimTypes.Role, "Member")
            };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    return RedirectToAction("Homepage", "Home");
                }
                ModelState.AddModelError("", "Invalid login attempt. Username or password is incorrect.");
            }

            if (!string.IsNullOrEmpty(model.RegisterViewModel?.Username) &&
                !string.IsNullOrEmpty(model.RegisterViewModel?.Password) &&
                !string.IsNullOrEmpty(model.RegisterViewModel?.Name) &&
                !string.IsNullOrEmpty(model.RegisterViewModel?.Surname) &&
                !string.IsNullOrEmpty(model.RegisterViewModel?.Email))
            {
                var username = model.RegisterViewModel.Username.ToLower();
                var email = model.RegisterViewModel.Email;

                if (username == "admin")
                {
                    ModelState.AddModelError(nameof(model.RegisterViewModel.Username), "Username 'admin' is not allowed.");
                    return View(model);
                }

                var existingUser = await _context.Members
                    .FirstOrDefaultAsync(m => m.Username == username);
                if (existingUser != null)
                {
                    ModelState.AddModelError(nameof(model.RegisterViewModel.Username), "Username is already taken.");
                    return View(model);
                }

                // Check if email already exists
                var existingEmail = await _context.Members
                    .FirstOrDefaultAsync(m => m.Email == email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError(nameof(model.RegisterViewModel.Email), "Email is already taken.");
                    return View(model);
                }

                var member = new Member
                {
                    Username = username,
                    Name = model.RegisterViewModel.Name,
                    Surname = model.RegisterViewModel.Surname,
                    Email = email,
                    Password = model.RegisterViewModel.Password,
                    IsActive = 1 // or any default value
                };

                _context.Members.Add(member);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login", "Account");
            }
            ModelState.AddModelError("", "Please fill in all required fields.");
            return View(model);
        }


        private async Task SignInUser(string username)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        }

        #endregion

        #region MyOrders
        public async Task<IActionResult> MyOrders()
        {
            var userName = User.Identity.Name;
            if (userName == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Username == userName);
            if (member == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (member.MemberID == null)
            {
                return BadRequest("MemberID is null");
            }

            var memberOrders = await _context.MemberOrders
                .Where(o => o.MemberID == member.MemberID)
                .ToListAsync();

            if (memberOrders == null)
            {
                return NotFound("No orders found for this member");
            }

            var memberOrderIds = memberOrders.Select(o => o.OrderID).ToList();
            var memberOrderProducts = _context.MemberOrderProducts
                .Where(op => memberOrderIds.Contains(op.OrderID))
                .ToList();

            var productIds = _context.MemberOrderProducts
                .Where(op => memberOrderIds.Contains(op.OrderID))
                .Select(op => op.ProductID)
                .Distinct()
                .ToList();
            
            var products =  _context.Products
                .Where(p => productIds.Contains(p.ProductID))
                .Select(p => new Product
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    Images = p.Images,
                    UnitPrice = p.UnitPrice
                })
                .ToList();
            var addresses = await _context.Addresses
                .Where(a => a.MemberID == member.MemberID)
                .ToListAsync();
            var shippers = await _context.Shippers.ToListAsync();
            var viewModel = new MyOrdersViewModel
            {
                MemberOrders = memberOrders,
                MemberOrderProducts = memberOrderProducts,
                Products = products,
                Addresses = addresses,
                Shippers = shippers
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Approve(int ShipperId, int AddressId,decimal totalPrice )
        {
            var userName = User.Identity.Name;
            if (userName == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Username == userName);

            if (member == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var shoppingBagItems = await _context.ShoppingBags
                .Where(sb => sb.UsernameID == member.MemberID)
                .ToListAsync();

            var shipper = await _context.Shippers
                .FirstOrDefaultAsync(s => s.ShipperID == ShipperId);

            if (shipper == null)
            {
                return BadRequest("Invalid shipper ID");
            }

            var firstDate = shipper.FirstDate;
            var lastDate = shipper.LastDate;

            if (firstDate >= lastDate)
            {
                return BadRequest("Invalid date range in shippers table");
            }

            var random = new Random();
            var day = random.Next(firstDate, lastDate);
            var randomDate = DateTime.Now.AddDays(day);
            var deliveryDate = new DateTime(randomDate.Year, randomDate.Month, randomDate.Day, 0, 0, 0);
            var ordercreated = DateTime.Now;
            var orderfee = totalPrice;
            string formattedOrderFee = FormatOrderFee(orderfee);
            var memberOrder = new MemberOrder
            {
                MemberID = member.MemberID,
                ShipperID = ShipperId,
                AddressID = AddressId,
                DeliveryDate = deliveryDate,
                OrderCreated = ordercreated,
                OrderFee = formattedOrderFee
            };

            _context.MemberOrders.Add(memberOrder);
            await _context.SaveChangesAsync();

            var orderProducts = shoppingBagItems.Select(item => new MemberOrderProduct
            {
                OrderID = memberOrder.OrderID,
                ProductID = item.ProductID,
                Quantity = item.Quantity
            }).ToList();

            _context.MemberOrderProducts.AddRange(orderProducts);

            _context.ShoppingBags.RemoveRange(shoppingBagItems);

            await _context.SaveChangesAsync();

            return RedirectToAction("MyOrders");
        }
        public static string FormatOrderFee(decimal orderfee)
        {
            // orderfee'yi 100'e bölerek son iki basamağı ayır
            decimal formattedValue = orderfee / 100;

            // Nokta ile ayırarak string olarak dön
            return formattedValue.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        }
        #endregion

        #region Address
        public async Task<IActionResult> Addresses()
        {
            var userName = User.Identity.Name;
            if (userName == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Username == userName);

            if (member == null)
            {
                return NotFound();
            }
            var addresses = await _context.Addresses
                .Where(a => a.MemberID == member.MemberID)
                .ToListAsync();

            return View(addresses);
        }
        [HttpPost]
        public async Task<IActionResult> AddAddress(Address address)
        {
            if (ModelState.IsValid)
            {
                var userName = User.Identity.Name;
                if (userName == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.Username == userName);

                if (member == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                address.MemberID = member.MemberID;

                _context.Add(address);
                await _context.SaveChangesAsync();

                return RedirectToAction("Addresses");
            }

            // If model state is invalid, return the form view with current data and validation errors
            return View("Addresses", address);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var userName = User.Identity.Name;
            if (userName == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Username == userName);

            if (member == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressID == id && a.MemberID == member.MemberID);

            if (address == null)
            {
                return NotFound();
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return RedirectToAction("Addresses");
        }
        [HttpPost]
        public async Task<IActionResult> EditAddress(Address address)
        {
            if (ModelState.IsValid)
            {
                var userName = User.Identity.Name;
                if (userName == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.Username == userName);

                if (member == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var existingAddress = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.AddressID == address.AddressID && a.MemberID == member.MemberID);

                if (existingAddress == null)
                {
                    return NotFound();
                }

                existingAddress.Street = address.Street;
                existingAddress.Apt = address.Apt;
                existingAddress.Province = address.Province;
                existingAddress.City = address.City;
                existingAddress.Neighborhood = address.Neighborhood;
                existingAddress.Postcode = address.Postcode;
                existingAddress.Country = address.Country;
                existingAddress.Phone = address.Phone;

                _context.Update(existingAddress);
                await _context.SaveChangesAsync();

                return RedirectToAction("Addresses");
            }
            return View(address);
        }
        #endregion

        #region UserInformation
        public async Task<IActionResult> UserInformation()
        {
            var userName = User.Identity.Name;
            var existingUser = await _context.Members.FirstOrDefaultAsync(m => m.Username == userName);

            return View(existingUser);
        }
        public async Task<IActionResult> UpdateInformation(Member model, IFormFile UserImage)
        {

            var existingUser = await _context.Members.FirstOrDefaultAsync(m => m.Username == model.Username);

            existingUser.Name = model.Name;
            existingUser.Surname = model.Surname;
            existingUser.Password = model.Password;
            existingUser.Email = model.Email;

            if (UserImage != null && UserImage.Length > 0)
            {
                var fileBytes = FileHelper.ReadFileBytes(UserImage);
                var hexString = FileHelper.ConvertToHex(fileBytes);

                existingUser.UserImage = hexString;
            }
            _context.Members.Update(existingUser);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserInformation");
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
        public static class ImageHelper
        {
            public static string ConvertHexToBase64(string hex)
            {
                byte[] bytes = Enumerable.Range(0, hex.Length)
                                          .Where(x => x % 2 == 0)
                                          .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                          .ToArray();

                return $"data:image/jpeg;base64,{Convert.ToBase64String(bytes)}";
            }
        }
        #endregion

    }



}