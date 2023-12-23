using BookStore.Data;
using BookStore.Models;
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

        public async Task<IActionResult> Items()
        {
            return View(await _context.Book.Include(m => m.Genres)
                                                          .Include(m => m.Author)
                                                                                                    .Include(m => m.Publisher).ToListAsync());
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