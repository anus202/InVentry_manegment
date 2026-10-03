using System.ComponentModel.DataAnnotations;

namespace Prectice_Interview.Data
{
    public class SaleItem
    {
        [Key] public int Id { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        [MaxLength(200)] public string ProductName { get; set; } = "";
        [MaxLength(50)] public string Sku { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal { get; set; }
    }
}
