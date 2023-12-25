using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Models;
using BookStore.Models.Binding_Model;
using Microsoft.AspNetCore.Authorization;

namespace BookStore.Controllers
{
	[Authorize(Roles = "admin")]
	public class BillsController : Controller
    {
        private readonly BookStoreContext _context;

        public BillsController(BookStoreContext context)
        {
            _context = context;
        }

        // GET: Bills
        public async Task<IActionResult> Index()
        {
            var bookStoreContext = _context.Bill.Include(b => b.User);
            return View(await bookStoreContext.ToListAsync());
        }

        // GET: Bills/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Bill == null)
            {
                return NotFound();
            }

            var bill = await _context.Bill
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }

        // GET: Bills/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            String userName = User.Identity.Name;
            var user = _context.Users.Include(u => u.Profile).SingleOrDefault(u => u.UserName == userName);
            var profile = user.Profile;
            ViewBag.UserName = profile?.FirstName == null || profile?.LastName == null ? userName : profile.FullName;
            int userId = user.Id;

            var bill = await _context.Bill.Include(x => x.OrderDetails)
                .ThenInclude(x => x.Book)
                .SingleOrDefaultAsync(or => or.UserId == profile.UserId && or.Status == OrderStatus.Cart);
            decimal total = bill.OrderDetails.Sum(x => x.Payment);
            ViewBag.Total = total.ToString();
            //var rank = _context.Ranks.SingleOrDefault(r => r.Id == user.Profile.RankId);
            //decimal discount = total * (rank.DiscountRate);
            decimal payment = total;
            //Console.WriteLine(rank.DiscountRate.ToString());
            var model = new BillBindingModel
            {
                Id = bill.Id,
                Phone = bill.Phone,
                Quantity = bill.Quantity,
                Address = bill.Address,
                Note = bill.Note,
                UserId = userId,
                OrderDetails = bill.OrderDetails,
                discount = 0,
                Payment = payment
            };
            return View(model);
        }

        // POST: Bills/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CreatedDate,Payment,Quantity,Address,Phone,Note,UserId,Status")] Bill bill)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", bill.UserId);
            return View(bill);
        }

        // GET: Bills/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Bill == null)
            {
                return NotFound();
            }

            var bill = await _context.Bill.FindAsync(id);
            if (bill == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", bill.UserId);
            return View(bill);
        }

        // POST: Bills/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CreatedDate,Payment,Quantity,Address,Phone,Note,UserId,Status")] Bill bill)
        {
            if (id != bill.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bill);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillExists(bill.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", bill.UserId);
            return View(bill);
        }

        // GET: Bills/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Bill == null)
            {
                return NotFound();
            }

            var bill = await _context.Bill
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }

        // POST: Bills/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Bill == null)
            {
                return Problem("Entity set 'BookStoreContext.Bill'  is null.");
            }
            var bill = await _context.Bill.FindAsync(id);
            if (bill != null)
            {
                _context.Bill.Remove(bill);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BillExists(int id)
        {
          return _context.Bill.Any(e => e.Id == id);
        }
    }
}
