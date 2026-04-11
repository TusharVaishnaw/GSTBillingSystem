using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceGST.API.Data;
using InvoiceGST.API.Models;
using InvoiceGST.API.Models.DTOs;
using InvoiceGST.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using QuestPDF.Fluent;
using InvoiceGST.API.Services;

namespace InvoiceGST.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InvoicesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;


        public InvoicesApiController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ===================== CREATE INVOICE =====================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateInvoice(CreateInvoiceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Validation failed", ModelState));
            }

            var customer = await _context.Customers.FindAsync(dto.CustomerId);

            if (customer == null)
                return NotFound(new ApiResponse<object>(false, "Customer not found", null));

            var currentYear = DateTime.Now.Year;

            var lastInvoice = await _context.Invoices
                .Where(i => i.InvoiceDate.Year == currentYear)
                .OrderByDescending(i => i.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastInvoice != null && lastInvoice.InvoiceNumber.Contains("-"))
            {
                var parts = lastInvoice.InvoiceNumber.Split('-');

                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            var formattedNumber = nextNumber.ToString("D4");

            var invoice = new Invoice
            {
                InvoiceNumber = $"INV-{currentYear}-{formattedNumber}",
                InvoiceDate = DateTime.Now,
                CustomerId = dto.CustomerId,
                Status = "Unpaid",
                InvoiceItems = new List<InvoiceItem>()
            };

            decimal subTotal = 0;

            foreach (var item in dto.Items)
            {
                var amount = item.Quantity * item.Rate;

                invoice.InvoiceItems.Add(new InvoiceItem
                {
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    Rate = item.Rate,
                    Amount = amount
                });

                subTotal += amount;
            }

            invoice.SubTotal = subTotal;
            invoice.CGST = subTotal * 0.09m;
            invoice.SGST = subTotal * 0.09m;
            invoice.IGST = 0;
            invoice.TotalAmount = subTotal + invoice.CGST + invoice.SGST;

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

        //    // ================= PDF GENERATE =================
        //     var pdfBytes = Document.Create(container =>
        //     {
        //         container.Page(page =>
        //         {
        //             page.Margin(30);

        //             page.Header().Text("GST Invoice").FontSize(20).Bold();

        //             page.Content().Column(col =>
        //             {
        //                 col.Item().Text($"Invoice Number: {invoice.InvoiceNumber}");
        //                 col.Item().Text($"Customer: {customer.Name}");
        //                 col.Item().Text($"Date: {invoice.InvoiceDate:dd-MM-yyyy}");

        //                 col.Item().LineHorizontal(1);

        //                 foreach (var item in invoice.InvoiceItems)
        //                 {
        //                     col.Item().Text($"{item.ProductName} - Qty:{item.Quantity} - ₹{item.Amount}");
        //                 }

        //                 col.Item().LineHorizontal(1);

        //                 col.Item().Text($"Subtotal: ₹{invoice.SubTotal}");
        //                 col.Item().Text($"CGST: ₹{invoice.CGST}");
        //                 col.Item().Text($"SGST: ₹{invoice.SGST}");
        //                 col.Item().Text($"Total: ₹{invoice.TotalAmount}");
        //             });
        //         });
        //     }).GeneratePdf();


        //     // ================= HTML EMAIL =================
        //     var html = $@"
        //     <h2 style='color:green;'>Invoice Generated</h2>
        //     <p>Hello {customer.Name},</p>
        //     <p>Your invoice <b>{invoice.InvoiceNumber}</b> has been created.</p>
        //     <p><b>Total Amount:</b> ₹{invoice.TotalAmount}</p>
        //     ";


        //     // ================= EMAIL SEND =================
        //     await _emailService.SendEmailWithAttachment(
        //         customer.Email,
        //         "Invoice Generated",
        //         html,
        //         pdfBytes,
        //         $"Invoice_{invoice.InvoiceNumber}.pdf"
        //     );

            // EMAIL YAHAN BHEJNA HAI
            // try
            // {
            //     await _emailService.SendEmail(
            //     customer.Email,
            //     "Invoice Generated",
            //     $"<h2>Invoice {invoice.InvoiceNumber} created successfully</h2><p>Total: ₹{invoice.TotalAmount}</p>"
            //   );

            // }
            // catch
            // {
            //     // ignore email failure (optional)

            // }
            
            // Audit Log
            _context.AuditLogs.Add(new AuditLog
            {
                UserEmail = User.Identity?.Name,
                Action = "Invoice Created",
                Entity = invoice.InvoiceNumber,
                TimeStamp = DateTime.Now
            });

            await _context.SaveChangesAsync();

            var response = new InvoiceResponseDto
            {
                InvoiceNumber = invoice.InvoiceNumber,
                SubTotal = invoice.SubTotal,
                CGST = invoice.CGST,
                SGST = invoice.SGST,
                TotalAmount = invoice.TotalAmount
            };

            return Ok(new ApiResponse<object>(true, "Invoice created successfully", response));
        }

        // ===================== GET BY ID =====================

        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound(new ApiResponse<object>(false, "Invoice not found", null));

            var data = new
            {
                invoice.InvoiceNumber,
                invoice.InvoiceDate,
                CustomerName = invoice.Customer.Name,
                invoice.SubTotal,
                invoice.CGST,
                invoice.SGST,
                invoice.IGST,
                invoice.TotalAmount,
                invoice.Status,
                Items = invoice.InvoiceItems.Select(item => new
                {
                    item.ProductName,
                    item.Quantity,
                    item.Rate,
                    item.Amount
                })
            };

            return Ok(new ApiResponse<object>(true, "Invoice fetched successfully", data));
        }

        // ===================== GET ALL =====================

        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<IActionResult> GetAllInvoices(int pageNumber = 1, int pageSize = 5)
        {
            var query = _context.Invoices
                .Include(i => i.Customer)
                .OrderByDescending(i => i.InvoiceDate);

            var totalRecords = await query.CountAsync();

            var invoices = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceNumber,
                    i.InvoiceDate,
                    CustomerName = i.Customer.Name,
                    i.TotalAmount,
                    i.Status
                })
                .ToListAsync();

            var result = new
            {
                totalRecords,
                pageNumber,
                pageSize,
                data = invoices
            };

            return Ok(new ApiResponse<object>(true, "Invoices fetched successfully", result));
        }

        // ===================== PDF =====================

        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DownloadInvoicePdf(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound(new ApiResponse<object>(false, "Invoice not found", null));

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header().Text("GST Invoice").FontSize(20).Bold();

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Invoice Number: {invoice.InvoiceNumber}");
                        col.Item().Text($"Customer: {invoice.Customer.Name}");
                        col.Item().Text($"Date: {invoice.InvoiceDate:dd-MM-yyyy}");

                        col.Item().LineHorizontal(1);

                        foreach (var item in invoice.InvoiceItems)
                        {
                            col.Item().Text($"{item.ProductName} - Qty:{item.Quantity} - ₹{item.Amount}");
                        }

                        col.Item().LineHorizontal(1);

                        col.Item().Text($"Subtotal: ₹{invoice.SubTotal}");
                        col.Item().Text($"CGST: ₹{invoice.CGST}");
                        col.Item().Text($"SGST: ₹{invoice.SGST}");
                        col.Item().Text($"Total: ₹{invoice.TotalAmount}");
                        col.Item().Text($"Status: {invoice.Status}");
                    });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"Invoice_{invoice.InvoiceNumber}.pdf");
        }

        // ===================== SEARCH =====================

        [Authorize(Roles = "Admin,User")]
        [HttpGet("search")]
        public async Task<IActionResult> SearchInvoices(
            int? customerId,
            string? status,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Invoices
                .Include(i => i.Customer)
                .AsQueryable();

            if (customerId.HasValue)
                query = query.Where(i => i.CustomerId == customerId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(i => i.Status == status);

            if (fromDate.HasValue)
                query = query.Where(i => i.InvoiceDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => i.InvoiceDate <= toDate.Value);

            var totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(i => i.InvoiceDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceNumber,
                    i.InvoiceDate,
                    Customer = i.Customer.Name,
                    i.TotalAmount,
                    i.Status
                })
                .ToListAsync();

            var result = new
            {
                totalRecords,
                page,
                pageSize,
                data
            };

            return Ok(new ApiResponse<object>(true, "Search results fetched", result));
        }

            // ==================email sent==============================

        [HttpPost("{id}/send-email")]
        public async Task<IActionResult> SendInvoiceEmail(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound("Invoice not found");

            // 🔥 PDF
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header().Text("GST Invoice").FontSize(20).Bold();

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Invoice Number: {invoice.InvoiceNumber}");
                        col.Item().Text($"Customer: {invoice.Customer.Name}");

                        foreach (var item in invoice.InvoiceItems)
                        {
                            col.Item().Text($"{item.ProductName} - ₹{item.Amount}");
                        }

                        col.Item().Text($"Total: ₹{invoice.TotalAmount}");
                    });
                });
            }).GeneratePdf();

            // 🔥 HTML
            var html = $@"
                <h2>Invoice</h2>
                <p>Hello {invoice.Customer.Name}</p>
                <p>Invoice: {invoice.InvoiceNumber}</p>
                <p>Total: ₹{invoice.TotalAmount}</p>
            ";

            await _emailService.SendEmailWithAttachment(
                invoice.Customer.Email,
                "Invoice PDF",
                html,
                pdfBytes,
                $"Invoice_{invoice.InvoiceNumber}.pdf"
            );

            return Ok("Email sent successfully");
        }
    }
}