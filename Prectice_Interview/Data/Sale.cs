using Prectice_Interview.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prectice_Interview.Data
{
    public class Sale : CommonField
    {
        [Key] public int Id { get; set; }
        [MaxLength(30)] public string InvoiceNo { get; set; } = "";
        public DateTime SaleDate { get; set; } = DateTime.Now;
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public SaleStatus Status { get; set; } = SaleStatus.Completed;
        [MaxLength(500)] public string? Notes { get; set; }
        [MaxLength(100)] public string? CashierName { get; set; }
        public List<SaleItem> Items { get; set; } = new();

        [NotMapped] public decimal Balance => Status == SaleStatus.Voided ? 0 : Total - PaidAmount;
    }
}
