using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
	public class CartsController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly BookStoreContext _context;

		public CartsController(BookStoreContext context, ILogger<HomeController> logger)
		{
			_context = context;
			_logger = logger;
		}
		[Authorize]
		public async Task<IActionResult> AddToCartWithQuantity(int quantity, int id)
		{
			var book = await _context.Book.FindAsync(id);
			var user = _context.Users.Include(u => u.Profile).SingleOrDefault(u => u.UserName == User.Identity.Name);
			var userId = user.Id;
			var cart = _context.Cart.Include(o => o.OrderDetails).FirstOrDefault(c => c.UserId == user.Id);

			if (cart == null)
			{
				cart = new Cart { UserId = user.Id, OrderDetails = new List<Order_Details>() };
				_context.Cart.Add(cart);
			}

			var cartItem = cart.OrderDetails.Where(ci => ci.CartId == cart.Id).ToList();
			if (cartItem != null)
			{
				var totalPrice = quantity * book.Price;
				foreach (var item in cartItem)
				{
					if (item.BookId == id)
					{
						var temp = item.Quantity;
						temp += quantity;
						if (temp > book.Quantity)
						{
							return RedirectToAction("Index", "Home");
						}
						item.Quantity = temp;
						totalPrice = item.Quantity * book.Price;
						item.TotalPrice = totalPrice;
						_context.SaveChanges();
						return RedirectToAction("Index", "Home");
					}
				}
				cart.OrderDetails.Add(new Order_Details
				{
					CartId = cart.Id,
					BookId = book.Id,
					Book = book,
					Quantity = quantity,
					TotalPrice = totalPrice,
				}); ;
			}
			else
			{
				var totalPrice = quantity * book.Price;
				cart.OrderDetails.Add(new Order_Details
				{
					CartId = cart.Id,
					BookId = book.Id,
					Book = book,
					Quantity = quantity,
					TotalPrice = totalPrice,
				});
			}
			_context.SaveChanges();
			return RedirectToAction("Index", "Home");
		}

		public async Task<IActionResult> Delete(int CartId, int BookId)
		{
			var CartItem = await _context.Order_Details.Include(o => o.Book).Where(x => x.CartId == CartId && x.BookId == BookId).FirstOrDefaultAsync();
				
			if (CartItem == null)
			{
				return NotFound();
			}

			return View(CartItem);
		}

		// POST: Books/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int CartId, int BookId)
		{
			if (_context.Cart == null)
			{
				return Problem("Entity set 'BookStoreContext.Cart'  is null.");
			}
			var CartItem = await _context.Order_Details.Where(x => x.CartId == CartId && x.BookId == BookId).FirstOrDefaultAsync();

			if (CartItem != null)
			{
				_context.Order_Details.Remove(CartItem);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction("Cart", "Home");
		}

		private bool BookExists(int id)
		{
			return _context.Book.Any(e => e.Id == id);
		}
	}
}
