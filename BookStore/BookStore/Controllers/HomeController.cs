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
		public async Task<IActionResult> Items(
											   string sort,
											   string currentSort,
											   string sortOrder,
											   string currentOrder,
											   string currentFilter,
											   string searchString,
											   int? pageNumber,
											   int[] sortGenres,
											   int[] sortAuthors,
											   int[] sortPublishers
											   )
		{

			if (sortOrder != null)
			{
				pageNumber = 1;
			}
			else
			{
				sortOrder = currentOrder;
			}
			if (sort != null)
			{
				pageNumber = 1;
			}
			else
			{
				sort = currentSort;
			}
			if (searchString != null)
			{
				pageNumber = 1;
			}
			else
			{
				searchString = currentFilter;
			}
			ViewData["CurrentFilter"] = searchString;
			ViewData["CurrentSort"] = sort;
			ViewData["CurrentOrder"] = sortOrder;
			ViewData["sortGenres"] = System.Text.Json.JsonSerializer.Serialize(sortGenres);
			ViewData["sortAuthors"] = System.Text.Json.JsonSerializer.Serialize(sortAuthors);
			ViewData["sortPublishers"] = System.Text.Json.JsonSerializer.Serialize(sortPublishers);
			var books = from s in _context.Book
						select s;
			if (!String.IsNullOrEmpty(searchString))
			{
				books = books.Where(s => s.Title.Contains(searchString));
			}
			if (!String.IsNullOrEmpty(sort))
			{
				string value = sort + sortOrder;
				switch (value)
				{
					case "aphalbeticalascending":
						books = books.OrderBy(s => s.Title);
						break;
					case "aphalbeticaldescending":
						books = books.OrderByDescending(s => s.Title);
						break;
					case "priceascending":
						books = books.OrderBy(s => s.Price);
						break;
					case "pricedescending":
						books = books.OrderByDescending(s => s.Price);
						break;
				}
			}
			foreach (var genreId in sortGenres)
			{
				books = books.Where(s => s.Genres.Any(g => g.Id == genreId));
			}
			foreach (var authorId in sortAuthors)
			{
				books = books.Where(s => s.Author.Id == authorId);
			}
			foreach (var publisherId in sortPublishers)
			{
				books = books.Where(s => s.Publisher.Id == publisherId);
			}
			int pageSize = 12;
			ViewBag.Genres = _context.Genre.ToList();
			ViewBag.Authors = _context.Author.ToList();
			ViewBag.Publishers = _context.Publisher.ToList();
			return View(await PaginatedList<Book>.CreateAsync(books.Include(x => x.Genres).Include(x => x.Author)
						.Include(x => x.Publisher).AsNoTracking(), pageNumber ?? 1, pageSize));
		}

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
										  .FirstOrDefaultAsync(o => o.IndexTemp == user.Id);
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
			return View(cart);
		}

		[Authorize]
		public async Task<IActionResult> AddToCart(int id)
		{
			var book = await _context.Book.FindAsync(id);
			var user = _context.Users.Include(u => u.Profile).SingleOrDefault(u => u.UserName == User.Identity.Name);
			var userId = user.Id;
			var cart = _context.Cart.Include(o => o.OrderDetails).FirstOrDefault(c => c.IndexTemp == user.Id);

			if (cart == null)
			{
				cart = new Cart { UserId = user.Id, OrderDetails = new List<Order_Details>() };
				cart.IndexTemp = cart.UserId;
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