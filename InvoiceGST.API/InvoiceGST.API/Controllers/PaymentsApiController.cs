using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceGST.API.Data;
using InvoiceGST.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace InvoiceGST.API.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admin can access payments API
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= ADD PAYMENT =================

        [HttpPost]
        public async Task<IActionResult> AddPayment(int invoiceId, decimal amount, string paymentMode)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
                return NotFound("Invoice not found");

            if (amount <= 0)
                return BadRequest("Payment amount must be greater than zero");

           var outstanding = invoice.TotalAmount - invoice.PaidAmount;

            if (amount > outstanding)
                return BadRequest("Payment exceeds outstanding amount");

            var payment = new Payment
            {
                InvoiceId = invoiceId,
                AmountPaid = amount,
                PaymentDate = DateTime.Now,
                PaymentMode = paymentMode
            };

            // Update amounts
            invoice.PaidAmount += amount;
            invoice.OutstandingAmount = invoice.TotalAmount - invoice.PaidAmount;

            // Update invoice status
            if (invoice.PaidAmount <= 0)
                invoice.Status = "Unpaid";
            else if (invoice.PaidAmount < invoice.TotalAmount)
                invoice.Status = "Partial";
            else
                invoice.Status = "Paid";

            var log = new AuditLog
            {
                UserEmail = User.Identity?.Name,
                Action = "Payment Added",
                Entity = invoice.InvoiceNumber,
                TimeStamp = DateTime.Now
            };

            _context.AuditLogs.Add(log);

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                invoice.Id,
                invoice.InvoiceNumber,
                invoice.TotalAmount,
                invoice.PaidAmount,
                invoice.OutstandingAmount,
                invoice.Status
            });
        }

        // ================= GET PAYMENTS =================

        [HttpGet("{invoiceId}")]
        public async Task<IActionResult> GetPaymentsByInvoice(int invoiceId)
        {
            var payments = await _context.Payments
                .Where(p => p.InvoiceId == invoiceId)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new
                {
                    p.Id,
                    p.AmountPaid,
                    p.PaymentDate,
                    p.PaymentMode
                })
                .ToListAsync();

            if (!payments.Any())
                return NotFound("No payments found for this invoice");

            return Ok(payments);
        }
    }
}