using BookStore.Data;
using BookStore.Migrations;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BookStore.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BookStoreContext _context;

        public HomeController(BookStoreContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Book.Include(m => m.Genres)
                                           .Include(m => m.Author)
                                           .Include(m => m.Publisher).ToListAsync());
        }
        [AllowAnonymous]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null || _context.Book == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(m => m.Genres)
                .Include(m => m.Author)
                .Include(m => m.Publisher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            ViewBag.Price = book.Price;
            ViewBag.Quantity = book.Quantity;
            ViewData["Genres"] = book.Genres.ToList();
            return View(book);
        }

        public async Task<IActionResult> Cart(int id)
        {
            var book = await _context.Book.FindAsync(id);
			var order = _context.Order.Include(x => x.Book).FirstOrDefault(x => x.Book.Id == id);
			var user = _context.Users.Include(u => u.Profile).SingleOrDefault(u => u.UserName == User.Identity.Name);
			var bill = await _context.Bill.SingleOrDefaultAsync(order => order.UserId == user.Id && order.Status == OrderStatus.Cart);
            if (bill != null)
            {

            } 
            else
            {
				bill = new Bill()
				{
					OrderDetails = new List<Order>(),
					UserId = user.Id,
					Status = OrderStatus.Cart
				};
				_context.Bill.Add(bill);
				await _context.SaveChangesAsync();
			}
			if (order != null)
            {
                int quantity = order.Quantity + 1;
                decimal payment = order.Book.Price;
                order.Quantity = quantity;
                order.Payment = (payment * quantity);
            }
            else
            {
                order = new Order()
                {
                    Quantity = 1,
                    Book = book,
                    Payment = book.Price,
                };
                order.BillId = bill.Id;
                order.Bill = bill;
            }
			_context.Order.Update(order);
			await _context.SaveChangesAsync();
			return Redirect(Url.Action("Index", "Home"));
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}