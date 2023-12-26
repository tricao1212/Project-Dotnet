using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly BookStoreContext _context;

        public OrdersController(BookStoreContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View(_context.Orders.ToList());
        }
        public async Task<IActionResult> OrderDetails(int Id)
        {
            var order = await _context.Orders.FindAsync(Id);
            var details = await _context.Order_Details.Include(o => o.Book).Where(d => d.CartId == order.CartId).ToListAsync();
            ViewBag.OrderDetails = details;
            return View(order);
        }

        public async Task<IActionResult> OrdersHistory()
        {
            var user = _context.Users.Include(u => u.Profile).SingleOrDefault(u => u.UserName == User.Identity.Name);
            var myOrder = await _context.Orders.Where(c => c.UserId == user.Id).ToListAsync();
            return View(myOrder);
        }

        public async Task<IActionResult> Order()
        {
            var user = _context.Users.Include(u => u.Profile).SingleOrDefault(u => u.UserName == User.Identity.Name);
            Profile profile = await _context.Profile.FindAsync(user.Id);
            var order = await _context.Cart.Include(c => c.OrderDetails)
                                          .ThenInclude(o => o.Book)
                                          .SingleOrDefaultAsync(or => or.IndexTemp == profile.UserId);
            ViewBag.OrderDetails = order.OrderDetails.ToList();
            var model = new OrderBindingModel
            {
                UserId = user.Id,
                CartId = order.Id,
                FirstName = profile?.FirstName == null ? "" : profile.FirstName,
                LastName = profile?.LastName == null ? "" : profile.LastName,
                Address = profile?.Address == null ? "" : profile.Address,
                Phone = profile?.PhoneNumber == null ? "" : profile.PhoneNumber,
                Note = "",
                TotalPrice = order.OrderDetails.Sum(o => o.TotalPrice)
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Order([Bind("UserId,CartId,FirstName,LastName,TotalPrice,Address,Phone,Note,CreatedAt")] Orders order)
        {
            if (ModelState.IsValid)
            {
                order.CreatedAt = DateTime.Now;

                _context.Add(order);
                var cart = await _context.Cart.FirstOrDefaultAsync(c => c.Id == order.CartId);
                cart.IndexTemp = -1;
                await _context.SaveChangesAsync();
                return View("OrderResult");
            }
            return View("OrderResult");
        }
    }
}