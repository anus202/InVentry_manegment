using Prectice_Interview.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prectice_Interview.Data
{
    public class AppUser : CommonField
    {
        [Key] public int Id { get; set; }
        [MaxLength(100)] public string FullName { get; set; } = "";
        [MaxLength(50)] public string Username { get; set; } = "";
        [MaxLength(300)] public string PasswordHash { get; set; } = "";
        public UserRole Role { get; set; } = UserRole.Cashier;
        public DateTime? LastLoginDate { get; set; }

        [NotMapped] public string? NewPassword { get; set; }
    }
}
