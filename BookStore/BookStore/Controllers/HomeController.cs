using BookStore.Data;
using BookStore.Migrations;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
            if (_context.Ranks.Count() == 0)
            {
                Rank.EnsureSeedData(_context);
            }
            return View(await _context.Book.Include(m => m.Genres)
                                           .Include(m => m.Author)
                                           .Include(m => m.Publisher).ToListAsync());
        }
        [AllowAnonymous]
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
			var user = await _context.Users.Include(u => u.Profile).SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
			var bill = await _context.Bill.SingleOrDefaultAsync(order => order.UserId == user.Id && order.Status == OrderStatus.Cart);
            if (bill == null)
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
			var order = _context.Order.Include(x => x.Book).Where(x=>x.BillId==bill.Id).FirstOrDefault(x => x.Book.Id == id);
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