using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace BookStore.Controllers
{
    public class OrdersController : Controller
    {
        private readonly BookStoreContext _context;

        public OrdersController(BookStoreContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
              return View(await _context.Order.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Order == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Quantity,Payment,BillId")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Order == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Quantity,Payment,BillId")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Cart));
            }
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Order == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Order == null)
            {
                return Problem("Entity set 'BookStoreContext.Order'  is null.");
            }
            var order = await _context.Order.FindAsync(id);
            if (order != null)
            {
                _context.Order.Remove(order);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Cart));
        }

        private bool OrderExists(int id)
        {
          return _context.Order.Any(e => e.Id == id);
        }
        [Authorize]
        public async Task<IActionResult> Cart()
        {
            String userName = User.Identity.Name;
            var user = _context.Users.Include(u => u.Profile).SingleOrDefault(u => u.UserName == userName);
            int userId = user.Id;
            decimal totalBill = 0;
            var cart = await _context.Bill
                                .Include(x => x.OrderDetails)
                                .ThenInclude(d => d.Book)
                                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.Cart);
            if (cart == null)
            {
                ViewBag.Order = new List<Order>();
            }
            else
            {
                ViewBag.Order = cart.OrderDetails.ToList();
                totalBill = cart.OrderDetails.Sum(x => x.Payment);
            }
            ViewBag.Total = totalBill;
            return View();
        }

        public async Task<IActionResult> Order(int id)
        {
            var book = await _context.Book.FindAsync(id);
            ViewBag.Book = book.Title;
            ViewBag.Price = book.Price;
            ViewBag.Quantity = book.Quantity;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Order([Bind("Quantity,Payment,Book,UserId")] Order orderDetail, int id)
        {
            Book book = await _context.Book.FindAsync(id);
            orderDetail.Book = book;
            var oldOrder = _context.Order.Include(x => x.Book).FirstOrDefault(x => x.Book.Id == id);
            var user = _context.Users.Include(u => u.Profile).SingleOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                decimal payment = orderDetail.Book.Price;
                int quantity = orderDetail.Quantity;
                if (oldOrder != null)
                {
                    quantity += oldOrder.Quantity;
                    if (quantity>oldOrder.Book.Quantity)
                        quantity = oldOrder.Book.Quantity;
                    oldOrder.Quantity = quantity;
                    oldOrder.Payment = (payment * quantity);
                }
                
                orderDetail.Payment = (payment * quantity);
                
                var bill = await _context.Bill.SingleOrDefaultAsync(order => order.UserId == user.Id && order.Status == OrderStatus.Cart);
                if (bill != null)
                {
                    if (oldOrder != null)
                    {

                        _context.Order.Update(oldOrder);
                    } 
                    else
                    {
                        orderDetail.BillId = bill.Id;
                        orderDetail.Bill = bill;
                        _context.Order.Add(orderDetail);
                    }
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
                    //Luôn duy tri 1 bill ở trạng thái cart: 
                    //Chưa có bill: new Bill()
                    //Có rồi. Add OrderDetail vào Bill                 
                    orderDetail.BillId = bill.Id;
                    orderDetail.Bill = bill;
                    _context.Order.Add(orderDetail);
                }
                await _context.SaveChangesAsync();
                return Redirect(Url.Action("Index", "Home"));
            }
            return View(orderDetail);
        }
    }
}
