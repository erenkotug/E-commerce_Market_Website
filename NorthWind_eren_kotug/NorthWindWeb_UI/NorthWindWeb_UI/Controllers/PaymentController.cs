using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthWind_DataAccessLayer.Data;
using NorthWind_DataAccessLayer.Entities;
using NorthWindWeb_UI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace NorthWindWeb_UI.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Payment()
        {
            var username = User.Identity.Name;
            if (username == null)
            {
                return Unauthorized();
            }

            var existingUser = await _context.Members
                .FirstOrDefaultAsync(m => m.Username == username);
            if (existingUser == null)
            {
                return NotFound();
            }

            var userId = existingUser.MemberID;

            // Retrieve ShoppingBags with ProductID and Quantity
            var shoppingBagItems = await _context.ShoppingBags
                .Where(sb => sb.UsernameID == userId)
                .Select(sb => new
                {
                    sb.ProductID,
                    sb.Quantity
                })
                .ToListAsync();

            // Retrieve only the products that are in the shopping bags
            var productIds = shoppingBagItems.Select(item => item.ProductID).Distinct().ToList();
            var productsWithPrices = await _context.Products
                .Where(p => productIds.Contains(p.ProductID))
                .ToDictionaryAsync(p => p.ProductID, p => p.UnitPrice);

            // Calculate the total sum
            decimal? totalSum = 0;
            foreach (var item in shoppingBagItems)
            {
                if (productsWithPrices.TryGetValue(item.ProductID, out var unitPrice))
                {
                    totalSum += unitPrice * item.Quantity;
                }
            }

            var addresses = await _context.Addresses
                .Where(a => a.MemberID == userId)
                .ToListAsync();

            var savedPayments = await _context.Payments
                .Where(a => a.MemberID == userId)
                .ToListAsync();

            var shippers = await _context.Shippers
                .ToListAsync();

            var viewModel = new PaymentViewModel
            {
                ShoppingBags = shoppingBagItems.Select(item => new ShoppingBagItemViewModel
                {
                    Product = productsWithPrices.ContainsKey(item.ProductID) ? new Product { ProductID = item.ProductID, UnitPrice = productsWithPrices[item.ProductID] } : null,
                    Quantity = item.Quantity
                }).ToList(),
                Addresses = addresses,
                SavedPayments = savedPayments,
                SavedShippers = shippers,
                TotalSum = totalSum
            };

            return View(viewModel);
        }


        public class ShoppingBagItemViewModel
        {
            public Product Product { get; set; }
            public int Quantity { get; set; }
        }
        public async Task<IActionResult> Save_Card()
        {
            var username = User.Identity.Name;
            if (username == null)
            {
                return Unauthorized();
            }
            var existingUser = await _context.Members
                .FirstOrDefaultAsync(m => m.Username == username);
            if (existingUser == null)
            {
                return NotFound();
            }
            var userId = existingUser.MemberID;
            var payments = await _context.Payments
                .Where(p => p.MemberID == userId)
                .ToListAsync();

            var model = new PaymentViewModel
            {
                Payments = payments,
                NewPayment = new Payment()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddCard(Payment payment)
        {
            if (ModelState.IsValid)
            {
                var username = User.Identity.Name;
                if (username == null)
                {
                    return Unauthorized();
                }

                var existingUser = await _context.Members
                    .FirstOrDefaultAsync(m => m.Username == username);
                if (existingUser == null)
                {
                    return NotFound();
                }


                payment.MemberID = existingUser.MemberID;

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Save_Card));
            }


            var model = new PaymentViewModel
            {
                Payments = await _context.Payments
                    .Where(p => p.MemberID == payment.MemberID)
                    .ToListAsync(),
                NewPayment = payment
            };

            ViewData["Error"] = "Kart eklenirken bir hata oluştu.";
            return View(nameof(Save_Card), model);
        }



        [HttpPost]
        public async Task<IActionResult> DeleteCard(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Save_Card));
        }
    }
}
