using System.ComponentModel.DataAnnotations;

public class CreateCustomerDto
{
    [Required]
    public string? Name { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [RegularExpression(@"^[0-9A-Z]{15}$", ErrorMessage = "Invalid GST Number")]
    public string? GSTNumber { get; set; }
}