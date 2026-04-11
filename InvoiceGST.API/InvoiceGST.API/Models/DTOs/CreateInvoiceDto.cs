namespace InvoiceGST.API.Models.DTOs
{
    public class CreateInvoiceDto
    {
        public int CustomerId { get; set; }

        public List<CreateInvoiceItemDto> Items { get; set; }
    }

    public class CreateInvoiceItemDto
    {
        public string ProductName { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal Rate { get; set; }
    }
}