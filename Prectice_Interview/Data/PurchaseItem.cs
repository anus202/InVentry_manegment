using System.ComponentModel.DataAnnotations;

namespace Prectice_Interview.Data
{
    public class PurchaseItem
    {
        [Key] public int Id { get; set; }
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        [MaxLength(200)] public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal { get; set; }
    }
}
