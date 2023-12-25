using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BookStore.Controllers
{
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
		public async Task<IActionResult> Details(int? id)
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

		[Authorize]
		public async Task<IActionResult> Cart()
		{
			var user = _context.Users.Include(u => u.Profile).SingleOrDefault(u => u.UserName == User.Identity.Name);
			decimal totalBill = 0;
			var cart = await _context.Cart.Include(x => x.OrderDetails)
										  .ThenInclude(d => d.Book)
										  .FirstOrDefaultAsync(o => o.UserId == user.Id);
			if (cart == null)
			{
				ViewBag.CartOrder = new List<Order_Details>();
			}
			else
			{
				ViewBag.CartOrder = cart.OrderDetails.ToList();
				totalBill = cart.OrderDetails.Sum(x => x.TotalPrice);
			}
			ViewBag.Total = totalBill;
			return View();
		}

		[Authorize]
		public async Task<IActionResult> AddToCart(int id)
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
				foreach (var item in cartItem)
				{
					if (item.BookId == id)
					{
						if (item.Quantity >= book.Quantity)
						{
							return RedirectToAction("Index", "Home");
						}
						item.Quantity++;
						item.TotalPrice += book.Price;
						_context.SaveChanges();
						return RedirectToAction("Index", "Home");
					}
				}
				cart.OrderDetails.Add(new Order_Details
				{
					CartId = cart.Id,
					BookId = book.Id,
					Book = book,
					Quantity = 1,
					TotalPrice = book.Price,
				}); ;
			}
			else
			{
				cart.OrderDetails.Add(new Order_Details
				{
					CartId = cart.Id,
					BookId = book.Id,
					Book = book,
					Quantity = 1,
					TotalPrice = book.Price,
				});
			}
			_context.SaveChanges();
			return RedirectToAction("Index", "Home");
		}

		public async Task<IActionResult> Index()
		{
			return View(await _context.Book.Include(m => m.Genres)
										   .Include(m => m.Author)
										   .Include(m => m.Publisher).ToListAsync());
		}
		public IActionResult YourAction(int page = 1, int pageSize = 4)
		{
			// Lấy dữ liệu từ nguồn của bạn (database, API, ...)
			var allItems = _context.Book.Include(m => m.Genres)
										.Include(m => m.Author)
										.Include(m => m.Publisher).ToList();

			// Tính toán phân trang
			var totalItems = allItems.Count();
			var items = allItems.Skip((page - 1) * pageSize).Take(pageSize);

			var model = new BookViewModel
			{
				Books = items,
				PagingInfo = new PagingInfo
				{
					CurrentPage = page,
					ItemsPerPage = pageSize,
					TotalItems = totalItems
				}
			};

			return View(model);
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