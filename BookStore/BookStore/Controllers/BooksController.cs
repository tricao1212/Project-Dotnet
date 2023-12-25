using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BookStore.Controllers
{
	public class BooksController : Controller
	{
		private readonly BookStoreContext _context;
		private readonly IWebHostEnvironment _iwebhost;


		public BooksController(BookStoreContext context, IWebHostEnvironment iwebhost)
		{
			_context = context;
			_iwebhost = iwebhost;

		}

		// GET: Books
		public async Task<IActionResult> Index(
		string sortOrder,
		string currentFilter,
		string searchString,
		int? pageNumber)
		{
			ViewData["CurrentSort"] = sortOrder;
			ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
			ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
			if (searchString != null)
			{
				pageNumber = 1;
			}
			else
			{
				searchString = currentFilter;
			}
			ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
			ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
			ViewData["CurrentFilter"] = searchString;
			var books = from s in _context.Book
						   select s;
			if (!String.IsNullOrEmpty(searchString))
			{
				books = books.Where(s => s.Title.Contains(searchString));
			}
			int pageSize = 4;
			return View(await PaginatedList<Book>.CreateAsync(books.Include(x=>x.Genres).Include(x => x.Author)
						.Include(x => x.Publisher).AsNoTracking(), pageNumber ?? 1, pageSize));
		}

		// GET: Books/Details/5
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
			ViewData["Genres"] = book.Genres.ToList();
			return View(book);
		}

		//GET: Books/Create
		public async Task<IActionResult> Create()
		{
			ViewData["Genres"] = await _context.Genre.ToListAsync();
			ViewData["AuthorId"] = new SelectList(_context.Author.ToList(),
				nameof(Author.Id), nameof(Author.Name));
			ViewData["PublisherId"] = new SelectList(_context.Publisher.ToList(),
				nameof(Publisher.Id), nameof(Publisher.Name));

			return View();
		}

		// POST: Books/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Title,Price,Quantity,imgFile,PublicDate,Description,AuthorId,PublisherId")] Book book, int[] selectedGenres)
		{
			if (ModelState.IsValid)
			{
				var fileName = Guid.NewGuid().ToString() + ".jpg";
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/book/", fileName);

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					await book.imgFile.CopyToAsync(fileStream);
				}

				if (selectedGenres != null)
				{
					book.Genres = _context.Genre.Where(x => selectedGenres.Contains(x.Id)).ToList();
				}
				book.ImgURL = "images/book/" + fileName;
				_context.Add(book);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["Genres"] = await _context.Genre.ToListAsync();
			ViewData["AuthorId"] = new SelectList(_context.Author.ToList(),
				   nameof(Author.Id), nameof(Author.Name));
			ViewData["PublisherId"] = new SelectList(_context.Publisher.ToList(),
				nameof(Publisher.Id), nameof(Publisher.Name));
			return View(book);
		}

		// GET: Books/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null || _context.Book == null)
			{
				return NotFound();
			}

			var book = await _context.Book.FindAsync(id);
			if (book == null)
			{
				return NotFound();
			}

			var books = await _context.Book.Include(x => x.Genres)
								.Include(x => x.Author)
								.SingleOrDefaultAsync(x => x.Id == id);
			ViewData["Genres"] = _context.Genre.ToList();
			ViewData["Author"] = new SelectList(_context.Author.ToList(), nameof(Author.Id), nameof(Author.Name));
			ViewData["Publisher"] = new SelectList(_context.Publisher.ToList(), nameof(Publisher.Id), nameof(Publisher.Name));
			return View(book);
		}

		// POST: Books/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Price,Quantity,imgFile,PublicDate,Description,AuthorId,PublisherId")] Book book, int[] selectedGenres)
		{
			if (id != book.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
                var bookToUpdate = await _context.Book
				.Include(i => i.Genres)
				.FirstOrDefaultAsync(s => s.Id == id);
                if (book.imgFile != null)
                {
                    var fileName = Guid.NewGuid().ToString() + ".jpg";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/book/", fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await book.imgFile.CopyToAsync(fileStream);
                    }
					bookToUpdate.ImgURL = "images/book/" + fileName;
                }
                if (await TryUpdateModelAsync<Book>(bookToUpdate, "", m => m.Title,m => m.Price, m => m.Quantity, m => m.PublicDate,
					m => m.AuthorId, m => m.PublisherId, m => m.Description))
				{
					await UpdateGenres(book, bookToUpdate, selectedGenres);
					try
					{
						await _context.SaveChangesAsync();
					}
					catch (DbUpdateConcurrencyException)
					{
						if (!BookExists(book.Id))
						{
							return NotFound();
						}
						else
						{
							throw;
						}
					}
				}
				return RedirectToAction(nameof(Index));
			}
			ViewData["Genres"] = _context.Genre.ToList();
			ViewData["Author"] = new SelectList(_context.Author.ToList(), nameof(Author.Id), nameof(Author.Name));
			ViewData["Publisher"] = new SelectList(_context.Publisher.ToList(), nameof(Publisher.Id), nameof(Publisher.Name));
			return View(book);
		}
		private async Task UpdateGenres(Book postedBook, Book movieToUpdate, int[] selectedGenres)
		{
			var genres = await _context.Genre.ToListAsync();
			foreach (var genre in movieToUpdate.Genres)
			{
				if (!selectedGenres.Any(x => x == genre.Id))
				{
					movieToUpdate.Genres.Remove(genre);
				}
			}

			foreach (var genreId in selectedGenres)
			{
				if (!movieToUpdate.Genres.Any(x => x.Id == genreId))
				{
					movieToUpdate.Genres.Add(genres.FirstOrDefault(x => x.Id == genreId));
				}
			}
		}

		// GET: Books/Delete/5
		public async Task<IActionResult> Delete(int? id)
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

			return View(book);
		}

		// POST: Books/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (_context.Book == null)
			{
				return Problem("Entity set 'BookStoreContext.Book'  is null.");
			}
			var book = await _context.Book.FindAsync(id);
			if (book != null)
			{
				_context.Book.Remove(book);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool BookExists(int id)
		{
			return _context.Book.Any(e => e.Id == id);
		}
	}
}
