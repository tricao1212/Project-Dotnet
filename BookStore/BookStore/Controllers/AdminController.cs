using BookStore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
    public class AdminController : Controller
    {
		private readonly BookStoreContext _context;
		public AdminController(BookStoreContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
        {
			var books = await _context.Book.ToListAsync();
			int quantity = 0;
			foreach(var book in books)
			{
				quantity += book.Quantity;	
			}
			return View(quantity);
        }
    }
}
