using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Data;

namespace Prectice_Interview.Services
{
    public class SaleRequest
    {
        public int? CustomerId { get; set; }
        public List<CartLine> Lines { get; set; } = new();
        public decimal DiscountAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Tendered { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public string? Notes { get; set; }
    }
}
