using Microsoft.AspNetCore.Mvc;
using InvoiceGST.API.Services;
using QuestPDF.Fluent;

namespace InvoiceGST.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailApiController : ControllerBase
    {
        private readonly EmailService _emailService;

        public EmailApiController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
public async Task<IActionResult> SendTestEmail(string toEmail)
{
    var html = @"
        <h1 style='color:green;'>Invoice Test Mail</h1>
        <p>This is professional email 🎉</p>
    ";

    var pdfBytes = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Content().Text("TEST PDF FROM EMAIL API");
        });
    }).GeneratePdf();

    await _emailService.SendEmailWithAttachment(
        toEmail,
        "Test Invoice Mail",
        html,
        pdfBytes,
        "test.pdf"
    );

    return Ok("Email with PDF sent");
}
    }
}