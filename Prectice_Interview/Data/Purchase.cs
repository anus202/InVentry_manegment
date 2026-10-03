using System.ComponentModel.DataAnnotations;

namespace Prectice_Interview.Data
{
    public class Purchase : CommonField
    {
        [Key] public int Id { get; set; }
        [MaxLength(30)] public string PurchaseNo { get; set; } = "";
        public DateTime PurchaseDate { get; set; } = DateTime.Today;
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        [MaxLength(100)] public string? Reference { get; set; }
        public decimal Total { get; set; }
        [MaxLength(500)] public string? Notes { get; set; }
        public List<PurchaseItem> Items { get; set; } = new();
    }
}
