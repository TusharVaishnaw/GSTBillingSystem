using System.ComponentModel.DataAnnotations;

namespace InvoiceGST.API.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        public Invoice Invoice { get; set; }

        [Required]
        public decimal AmountPaid { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentMode { get; set; }  // Cash / UPI / Bank
    }
}