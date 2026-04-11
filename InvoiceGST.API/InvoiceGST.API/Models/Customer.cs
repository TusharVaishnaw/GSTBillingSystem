namespace InvoiceGST.API.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? GSTNumber { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
    }
}