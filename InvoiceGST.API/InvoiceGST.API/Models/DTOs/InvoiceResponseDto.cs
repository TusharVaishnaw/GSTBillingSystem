namespace InvoiceGST.API.Models.DTOs
{
    public class InvoiceResponseDto
    {
        public string InvoiceNumber { get; set; } = null!;

        public decimal SubTotal { get; set; }

        public decimal CGST { get; set; }

        public decimal SGST { get; set; }

        public decimal TotalAmount { get; set; }
    }
}