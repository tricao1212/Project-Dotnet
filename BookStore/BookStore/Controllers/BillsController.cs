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
using System.Data;
using System.Net;

namespace BookStore.Controllers
{
    [Authorize]
    public class BillsController : Controller
    {
        private readonly BookStoreContext _context;

        public BillsController(BookStoreContext context)
        {
            _context = context;
        }

        // GET: Bills
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var bookStoreContext = _context.Bill.Include(b => b.User).Where(x => x.Status != OrderStatus.Cart);
            return View(await bookStoreContext.ToListAsync());
        }

        // GET: Bills/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            String userName = User.Identity.Name;
            var user = _context.Users.Include(u => u.Profile).SingleOrDefault(u => u.UserName == userName);
            var profile = user.Profile;
            ViewBag.UserName = profile?.FirstName == null || profile?.LastName == null ? userName : profile.FullName;
            var bill = await _context.Bill.Include(x => x.OrderDetails)
               .ThenInclude(x => x.Book)
               .SingleOrDefaultAsync(or => or.Id == id);

            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }
        public async Task<IActionResult> History()
        {
            String userName = User.Identity.Name;
            var user = _context.Users.Include(u => u.Profile).SingleOrDefault(u => u.UserName == userName);
            var profile = user.Profile;
            ViewBag.UserName = profile?.FirstName == null || profile?.LastName == null ? userName : profile.FullName;
            var bills = await _context.Bill.Where(x => x.Status != OrderStatus.Cart && x.UserId == user.Id).OrderByDescending(x => x.CreatedDate).ToListAsync();
            decimal total = bills.Sum(x => x.Payment);
            string totalString = string.Format("{0:C}", total);
            ViewBag.Total = totalString;
            return View(bills);
        }
        // GET: Bills/Create
        public async Task<IActionResult> Create()
        {
            String userName = User.Identity.Name;
            var user = _context.Users.Include(u => u.Profile).SingleOrDefault(u => u.UserName == userName);
            var profile = user.Profile;
            var rankId = profile.RankId;
            var userRank = await _context.Ranks.FindAsync(rankId);
            ViewBag.UserName = profile?.FirstName == null || profile?.LastName == null ? userName : profile.FullName;
            ViewBag.Ranks = userRank.Name;
            int userId = user.Id;
            var bill = await _context.Bill.Include(x => x.OrderDetails).ThenInclude(x => x.Book)
            .SingleOrDefaultAsync(or => or.UserId == profile.UserId && or.Status == OrderStatus.Cart);

            decimal total = bill.OrderDetails.Sum(x => x.Payment);
            ViewBag.Total = String.Format("{0:C}", total);
            //calculate discount
            decimal discount = total * (userRank.discount) / 100;
            //calculate payment
            decimal payment = total - discount;
            //add totalspent
            profile.TotalSpent += payment;
            //update rank
            List<Rank> ranks = _context.Ranks.ToList();
            foreach (var r in ranks)
            {
                if (profile.TotalSpent >= r.threadhold)
                {
                    profile.Rank = r;
                    await _context.SaveChangesAsync();
                }
            }


            var model = new BillBindingModel
            {
                Id = bill.Id,
                Phone = profile.PhoneNumber,
                Quantity = bill.Quantity,
                Address = profile.Address,
                Note = bill.Note,
                UserId = userId,
                OrderDetails = bill.OrderDetails,
                Discount = discount,
                Payment = payment
            };
            return View(model);
        }
        public async Task<IActionResult> GetCoupon(string code, decimal price)
        {
            var coupon = await _context.Coupon.SingleOrDefaultAsync(x => x.Giftcode.Equals(code));
            if (coupon != null)
            {
                if (coupon.ExpireDate > DateTime.Now)
                {
                    decimal discount = (price * coupon.Discount) / 100;
                    ViewBag.Payment = String.Format("{0:C}", discount);
                    decimal temp = price - discount;
                    ViewBag.temp = temp;
                    ViewBag.NewTotal = String.Format("{0:C}", temp);
                }
            }
            else
            {
                ViewBag.temp = price;
                ViewBag.OldTotal = String.Format("{0:C}", price);
            }
            return PartialView(coupon);
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
                String userName = User.Identity.Name;
                var users = _context.Users.Include(u => u.Profile).SingleOrDefault(u => u.UserName == userName);
                var profile = users.Profile;

                var findBill = await _context.Bill.SingleOrDefaultAsync(order => order.UserId == users.Id && order.Status == OrderStatus.Cart && order.Id == bill.Id);
                if (findBill != null)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == bill.UserId);
                    var cart = await _context.Bill
                               .Include(x => x.OrderDetails)
                               .ThenInclude(d => d.Book)
                               .FirstOrDefaultAsync(o => o.UserId == bill.UserId && o.Status == OrderStatus.Cart);
                    var temp = bill.Payment;

                    findBill.User = user;
                    findBill.Payment = temp;
                    findBill.Quantity = bill.Quantity;
                    bill.Status = OrderStatus.Ordered;
                    findBill.Status = bill.Status;
                    findBill.CreatedDate = DateTime.Now;
                    findBill.Address = bill.Address;
                    findBill.Note = bill.Note;
                    findBill.Phone = bill.Phone;
                    List<Order> orderDetails = cart.OrderDetails.ToList();
                    findBill.OrderDetails = orderDetails;
                    if (await UpdateQuantity(orderDetails))
                    {
                        //List<Models.Rank> ranks = _context.Ranks.ToList();
                        //Models.Rank rank = _context.Ranks.SingleOrDefault(r => r.Id == profile.RankId);

                        //UpdateRank(rank, ranks, profile, findBill);
                        //profile.TotalPayment += bill.Payment;
                        //_context.Update(profile);
                        _context.Update(findBill);
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Details", "Bills", new { id = findBill.Id });
                    }
                    else
                    {
                        return PartialView("CreateBillError");

                    }
                }
            }
            return View(bill);
        }
        private async Task<Boolean> UpdateQuantity(List<Order> orderDetails)
        {
            foreach (var item in orderDetails)
            {
                Book book = item.Book;
                if (book.Quantity < item.Quantity)
                {
                    _context.Order.Remove(item);
                    await _context.SaveChangesAsync();
                    return false;
                }
                book.Quantity -= item.Quantity;
                _context.Update(book);
            }
            return true;
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
            var enumList = Enum.GetValues(typeof(OrderStatus))
            .Cast<OrderStatus>()
            .Where(status => status != OrderStatus.Cart)
            .Select(status => new SelectListItem
            {
                Text = status.ToString(),
                Value = ((int)status).ToString()
            })
            .ToList();

            ViewBag.StatusList = enumList;
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
