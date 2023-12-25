using BookStore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;

namespace BookStore.Controllers
{
	[Authorize(Roles = "admin")]
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
            var orders = await _context.Bill.Where(s => s.Status != 0).ToListAsync();

            int quantity = 0;
            decimal earn = 0;
			foreach(var book in books)
			{
				quantity += book.Quantity;	
			}
            foreach(var order in orders)
            {
                earn += order.Payment;
            }
            ViewBag.Order = orders.Count;
			ViewBag.Quantity = quantity;
            ViewBag.Earn = earn.ToString();
            var dailyStats = await _context.Bill.Where(s => s.Status != 0).GroupBy(sale => sale.CreatedDate.Date)
                .Select(group => new
                {
                    OrderDate = group.Key,
                    OrderCount = group.Count(),
                    Revenue = group.Sum(sale => sale.Payment)
                })
                .OrderBy(stats => stats.OrderDate)
                .ToListAsync();
            var daily = new List<DailyStatisticsViewModel>();
            foreach (var item in dailyStats)
            {
                daily.Add(new DailyStatisticsViewModel
                {
                    OrderDate = item.OrderDate,
                    OrderCount = item.OrderCount,
                    Revenue = (double)item.Revenue

                });
            }
            var bookQuantities = await _context.Order.Include(od => od.Bill).Where(od => od.Bill.Status != 0)
                .Include(od => od.Book)
                .GroupBy(od => od.Book.Id)
             .Select(g => new
             {
                 BookName = g.FirstOrDefault().Book.Title,
                 TotalQuantity = g.Sum(od => od.Quantity)
             }).OrderBy(d => d.TotalQuantity)
            .ToListAsync();
            var chartData = new List<ChartItem>();
            foreach (var item in bookQuantities)
            {
                chartData.Add(new ChartItem
                {
                    Label = item.BookName,
                    Value = item.TotalQuantity
                });
            }
            ViewBag.ChartData = chartData;
            return View(daily);
        }

        public async Task<ActionResult> DailyStatistics(DateTime startDate, DateTime endDate)
        {
            // Group sales by order date and calculate statistics
            if (startDate == DateTime.MinValue || endDate == DateTime.MinValue)
            {
                var dailyStats2 = await _context.Bill.Where(s => s.Status != 0).GroupBy(sale => sale.CreatedDate.Date)
                .Select(group => new
                {
                    OrderDate = group.Key,
                    OrderCount = group.Count(),
                    Revenue = group.Sum(sale => sale.Payment)
                })
                                .OrderBy(stats => stats.OrderDate)
                                .ToListAsync();
                var daily2 = new List<DailyStatisticsViewModel>();
                foreach (var item in dailyStats2)
                {
                    daily2.Add(new DailyStatisticsViewModel
                    {
                        OrderDate = item.OrderDate,
                        OrderCount = item.OrderCount,
                        Revenue = (double)item.Revenue

                    });
                }
                return View(daily2);
            }
            var dailyStats = await _context.Bill.Where(s => s.Status != 0 && s.CreatedDate.Date >= startDate.Date && s.CreatedDate.Date <= endDate.Date).GroupBy(sale => sale.CreatedDate.Date)
                .Select(group => new
                {
                    OrderDate = group.Key,
                    OrderCount = group.Count(),
                    Revenue = group.Sum(sale => sale.Payment)
                })
                .OrderBy(stats => stats.OrderDate)
                .ToListAsync();
            var daily = new List<DailyStatisticsViewModel>();
            foreach (var item in dailyStats)
            {
                daily.Add(new DailyStatisticsViewModel
                {
                    OrderDate = item.OrderDate,
                    OrderCount = item.OrderCount,
                    Revenue = (double)item.Revenue

                });
            }
            return View(daily);
        }
        public async Task<ActionResult> TopProducts(DateTime startDate, DateTime endDate)
        {
            if (startDate == DateTime.MinValue || endDate == DateTime.MinValue)
            {
                var bookQuantity = await _context.Order.Include(od => od.Bill).Where(od => od.Bill.Status != 0).Include(od => od.Book)
                .GroupBy(od => od.Book.Id)
                 .Select(g => new
                 {
                     BookName = g.FirstOrDefault().Book.Title,
                     TotalQuantity = g.Sum(od => od.Quantity)
                 }).OrderBy(d => d.TotalQuantity)
                .ToListAsync();
                var chartData2 = new List<ChartItem>();
                foreach (var item in bookQuantity)
                {
                    chartData2.Add(new ChartItem
                    {
                        Label = item.BookName,
                        Value = item.TotalQuantity
                    });
                }

                return View(chartData2);
            }

            var bookQuantities = await _context.Order.Include(od => od.Bill).Where(od => od.Bill.Status != 0).Where(od => od.Bill.CreatedDate.Date >= startDate.Date && od.Bill.CreatedDate.Date <= endDate.Date)
                .Include(od => od.Book)
                .GroupBy(od => od.Book.Id)
             .Select(g => new
             {
                 BookName = g.FirstOrDefault().Book.Title,
                 TotalQuantity = g.Sum(od => od.Quantity)
             }).OrderBy(d => d.TotalQuantity)
            .ToListAsync();

            var chartData = new List<ChartItem>();
            foreach (var item in bookQuantities)
            {
                chartData.Add(new ChartItem
                {
                    Label = item.BookName,
                    Value = item.TotalQuantity
                });
            }

            return View(chartData);
        }
    }
}
