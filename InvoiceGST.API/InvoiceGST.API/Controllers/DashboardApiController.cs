using InvoiceGST.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvoiceGST.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var totalInvoices = await _context.Invoices.CountAsync();

            var totalRevenue = await _context.Invoices
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

            var totalGST = await _context.Invoices
                .SumAsync(i => (decimal?)(i.CGST + i.SGST + i.IGST)) ?? 0;

            var totalPaidAmount = await _context.Invoices
                .SumAsync(i => (decimal?)i.PaidAmount) ?? 0;

            var totalOutstanding = await _context.Invoices
                .SumAsync(i => (decimal?)i.OutstandingAmount) ?? 0;

            var paidCount = await _context.Invoices
                .CountAsync(i => i.Status == "Paid");

            var partialCount = await _context.Invoices
                .CountAsync(i => i.Status == "Partial");

            var unpaidCount = await _context.Invoices
                .CountAsync(i => i.Status == "Unpaid");

            return Ok(new
            {
                TotalInvoices = totalInvoices,
                TotalRevenue = totalRevenue,
                TotalGSTCollected = totalGST,
                TotalPaidAmount = totalPaidAmount,
                TotalOutstandingAmount = totalOutstanding,
                PaidInvoices = paidCount,
                PartialInvoices = partialCount,
                UnpaidInvoices = unpaidCount
            });
        }

        [HttpGet("monthly-sales")]
        public async Task<IActionResult> GetMonthlySales()
        {
            var data = await _context.Invoices
                .GroupBy(i => new { i.InvoiceDate.Year, i.InvoiceDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(x => x.TotalAmount),
                    TotalGST = g.Sum(x => x.CGST + x.SGST + x.IGST),
                    InvoiceCount = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("top-customers")]
        public async Task<IActionResult> GetTopCustomers()
        {
            var data = await _context.Invoices
                .Include(i => i.Customer)
                .GroupBy(i => i.Customer.Name)
                .Select(g => new
                {
                    CustomerName = g.Key,
                    TotalSpent = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(5)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("recent-invoices")]
        public async Task<IActionResult> GetRecentInvoices()
        {
            var data = await _context.Invoices
                .Include(i => i.Customer)
                .OrderByDescending(i => i.InvoiceDate)
                .Take(5)
                .Select(i => new
                {
                    i.InvoiceNumber,
                    i.InvoiceDate,
                    CustomerName = i.Customer.Name,
                    i.TotalAmount,
                    i.Status
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}