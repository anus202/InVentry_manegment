using Prectice_Interview.Enums;
using System.ComponentModel.DataAnnotations;

namespace Prectice_Interview.Data
{
    public class StockMovement
    {
        [Key] public int Id { get; set; }
        public int ProductId { get; set; }
        [MaxLength(200)] public string ProductName { get; set; } = "";
        public MovementType Type { get; set; }
        public int Quantity { get; set; }
        public int BalanceAfter { get; set; }
        [MaxLength(50)] public string? Reference { get; set; }
        [MaxLength(300)] public string? Note { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        [MaxLength(100)] public string? UserName { get; set; }
    }
}
