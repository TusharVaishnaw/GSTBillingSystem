namespace InvoiceGST.API.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }

        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        public string ProductName { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; }
    }
}