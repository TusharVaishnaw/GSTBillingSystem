using InvoiceGST.API.Models;

namespace InvoiceGST.API.Models
{
    public class Invoice
    {
        public Invoice()
        {
            InvoiceItems = new List<InvoiceItem>();
            Payments = new List<Payment>();
        }
        public int Id { get; set; }

        public string InvoiceNumber { get; set; }

        public DateTime InvoiceDate { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public decimal SubTotal { get; set; }

        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; }

        public decimal PaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }

        public ICollection<Payment> Payments { get; set; }

        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}