using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceGST.API.Data;

namespace InvoiceGST.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AuditApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuditApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(x => x.TimeStamp)
                .ToListAsync();

            return Ok(logs);
        }
    }
}