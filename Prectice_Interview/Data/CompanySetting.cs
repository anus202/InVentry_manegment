using System.ComponentModel.DataAnnotations;

namespace Prectice_Interview.Data
{
    public class CompanySetting : CommonField
    {
        [Key] public int Id { get; set; }
        [MaxLength(150)] public string CompanyName { get; set; } = "My Company";
        [MaxLength(300)] public string? Address { get; set; }
        [MaxLength(50)] public string? Phone { get; set; }
        [MaxLength(100)] public string? Email { get; set; }
        [MaxLength(50)] public string? TaxNumber { get; set; }
        [MaxLength(10)] public string Currency { get; set; } = "Rs.";
        public decimal TaxRate { get; set; }
        [MaxLength(20)] public string InvoicePrefix { get; set; } = "INV-";
        [MaxLength(300)] public string InvoiceFooter { get; set; } = "Thank you for your business!";
    }
}
