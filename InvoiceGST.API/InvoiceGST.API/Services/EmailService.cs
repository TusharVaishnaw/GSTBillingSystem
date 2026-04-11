using SendGrid;
using SendGrid.Helpers.Mail;

namespace InvoiceGST.API.Services
{
    public class EmailService
    {
        private readonly string _apiKey = "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";

        // SIMPLE EMAIL (NO PDF)
        public async Task SendEmail(string toEmail, string subject, string htmlContent)
        {
            var client = new SendGridClient(_apiKey);

            var from = new EmailAddress("dishika311@gmail.com", "Invoice App");
            var to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);

            await client.SendEmailAsync(msg);
        }

        // EMAIL WITH PDF ATTACHMENT
        public async Task SendEmailWithAttachment(
            string toEmail,
            string subject,
            string htmlContent,
            byte[] pdfBytes,
            string fileName)
        {
            var client = new SendGridClient(_apiKey);

            var from = new EmailAddress("dishika311@gmail.com", "Invoice App");
            var to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);

            // 🔥 FIXED ATTACHMENT
            var fileBase64 = Convert.ToBase64String(pdfBytes);

            msg.AddAttachment(new Attachment
            {
                Content = fileBase64,
                Filename = fileName,
                Type = "application/pdf",
                Disposition = "attachment"
            });

            var response = await client.SendEmailAsync(msg);
        }
    }
}