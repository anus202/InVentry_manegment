using System.ComponentModel.DataAnnotations;

namespace Prectice_Interview.Data
{
    public class Supplier : CommonField
    {
        [Key] public int Id { get; set; }
        [MaxLength(150)] public string Name { get; set; } = "";
        [MaxLength(100)] public string? ContactPerson { get; set; }
        [MaxLength(30)] public string? Phone { get; set; }
        [MaxLength(100)] public string? Email { get; set; }
        [MaxLength(300)] public string? Address { get; set; }
    }
}
