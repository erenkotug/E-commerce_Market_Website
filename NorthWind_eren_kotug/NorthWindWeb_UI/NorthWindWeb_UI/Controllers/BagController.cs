using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthWind.DataAccessLayer.Entities;
using NorthWind_DataAccessLayer.Data;
using NorthWindWeb_UI.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace NorthWindWeb_UI.Controllers
{
    public class BagController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public BagController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Default()
        {
            var username = User.Identity.Name;
            if (username == null)
            {
                return Unauthorized();
            }

            var existingUser = await _dbContext.Members
                .FirstOrDefaultAsync(m => m.Username == username);
            if (existingUser == null)
            {
                return NotFound();
            }

            var userId = existingUser.MemberID;

            var bagProducts = _dbContext.ShoppingBags
                .Where(sb => sb.UsernameID == userId)
                .Select(sb => new ProductViewModel
                {
                    ProductID = sb.ProductID,
                    ProductName = sb.Product.ProductName,
                    UnitPrice = sb.Product.UnitPrice,
                    Quantity = sb.Quantity,
                    Images = sb.Product.Images
                })
                .ToList();

            return View(bagProducts);
        }

        [HttpPost]
        public async Task<IActionResult> AddToBag(int productId)
        {
            var username = User.Identity.Name;
            if (username == null)
            {
                return Unauthorized();
            }

            var existingUser = await _dbContext.Members
                .FirstOrDefaultAsync(m => m.Username == username);
            if (existingUser == null)
            {
                return NotFound();
            }

            var userId = existingUser.MemberID;

            var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.ProductID == productId);

            if (product == null)
            {
                return NotFound(); // Product does not exist
            }

            if (product.UnitsInStock <= 0)
            {
                return BadRequest("Product is out of stock.");
            }

            var existingEntry = await _dbContext.ShoppingBags
                .FirstOrDefaultAsync(sb => sb.ProductID == productId && sb.UsernameID == userId);

            if (existingEntry == null)
            {
                var shoppingBagEntry = new ShoppingBag
                {
                    ProductID = productId,
                    UsernameID = userId,
                    Quantity = 1
                };
                _dbContext.ShoppingBags.Add(shoppingBagEntry);
            }
            else
            {
                existingEntry.Quantity++;
                _dbContext.ShoppingBags.Update(existingEntry);
            }

            await _dbContext.SaveChangesAsync();
            return Ok(); // Successful addition
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromBag(int productId)
        {
            var username = User.Identity.Name;
            if (username == null)
            {
                return Unauthorized();
            }

            var existingUser = await _dbContext.Members
                .FirstOrDefaultAsync(m => m.Username == username);
            if (existingUser == null)
            {
                return NotFound();
            }

            var userId = existingUser.MemberID;

            var shoppingBagEntry = _dbContext.ShoppingBags
                .FirstOrDefault(sb => sb.ProductID == productId && sb.UsernameID == userId);

            if (shoppingBagEntry != null)
            {
                _dbContext.ShoppingBags.Remove(shoppingBagEntry);
                _dbContext.SaveChanges();
            }

            return RedirectToAction("Default", "Bag");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var username = User.Identity.Name;
            if (username == null)
            {
                return Unauthorized();
            }

            var existingUser = await _dbContext.Members
                .FirstOrDefaultAsync(m => m.Username == username);
            if (existingUser == null)
            {
                return NotFound();
            }

            var userId = existingUser.MemberID;

            var shoppingBagEntry = await _dbContext.ShoppingBags
                .FirstOrDefaultAsync(sb => sb.ProductID == productId && sb.UsernameID == userId);

            if (shoppingBagEntry != null)
            {
                if (quantity <= 0)
                {
                    _dbContext.ShoppingBags.Remove(shoppingBagEntry);
                }
                else
                {
                    shoppingBagEntry.Quantity = quantity;
                    _dbContext.ShoppingBags.Update(shoppingBagEntry);
                }
                await _dbContext.SaveChangesAsync();
            }

            return Ok(); // Successful update
        }
    }
}
