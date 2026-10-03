using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prectice_Interview.Data;

namespace Prectice_Interview.Services
{
    public class AuthService
    {
        private const int Iterations = 100_000;
        private readonly IDbContextFactory<ApplicationDbContext> _factory;

        public AuthService(IDbContextFactory<ApplicationDbContext> factory) => _factory = factory;

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string stored)
        {
            var parts = stored.Split('.');
            if (parts.Length != 3 || !int.TryParse(parts[0], out var iter)) return false;
            var salt = Convert.FromBase64String(parts[1]);
            var expected = Convert.FromBase64String(parts[2]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iter, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        public async Task<bool> AnyUserAsync()
        {
            await using var db = await _factory.CreateDbContextAsync();
            return await db.Users.AnyAsync();
        }

        public async Task<AppUser> CreateUserAsync(string fullName, string username, string password, UserRole role)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var user = new AppUser { FullName = fullName.Trim(), Username = username.Trim(), PasswordHash = Hash(password), Role = role };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return user;
        }

        public async Task<AppUser?> LoginAsync(string username, string password)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username.Trim() && u.IsActive);
            if (user == null || !Verify(password, user.PasswordHash)) return null;
            user.LastLoginDate = DateTime.Now;
            await db.SaveChangesAsync();
            return user;
        }
    }
}
