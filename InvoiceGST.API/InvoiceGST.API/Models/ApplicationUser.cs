using Microsoft.AspNetCore.Identity;

namespace InvoiceGST.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}