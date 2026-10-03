using System.ComponentModel.DataAnnotations;

namespace Prectice_Interview.Data
{
    public class Category : CommonField
    {
        [Key] public int Id { get; set; }
        [MaxLength(100)] public string Name { get; set; } = "";
        [MaxLength(300)] public string? Description { get; set; }
    }
}
