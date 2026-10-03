using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prectice_Interview.Data
{
    public class Product : CommonField
    {
        [Key] public int Id { get; set; }
        [MaxLength(50)] public string Sku { get; set; } = "";
        [MaxLength(50)] public string? Barcode { get; set; }
        [MaxLength(200)] public string Name { get; set; } = "";
        [MaxLength(500)] public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        [MaxLength(20)] public string Unit { get; set; } = "pcs";
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public int StockQty { get; set; }
        public int ReorderLevel { get; set; } = 5;

        [NotMapped] public bool IsLowStock => StockQty <= ReorderLevel;
    }
}
