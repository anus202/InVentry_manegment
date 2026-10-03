using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prectice_Interview.Data;

namespace Prectice_Interview.Services
{
    public class SettingsService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _factory;
        public CompanySetting Current { get; private set; } = new();

        public SettingsService(IDbContextFactory<ApplicationDbContext> factory) => _factory = factory;

        public async Task LoadAsync()
        {
            await using var db = await _factory.CreateDbContextAsync();
            var s = await db.CompanySettings.AsNoTracking().FirstOrDefaultAsync();
            if (s == null)
            {
                s = new CompanySetting();
                db.CompanySettings.Add(s);
                await db.SaveChangesAsync();
            }
            Current = s;
            Fmt.Currency = s.Currency;
        }

        public async Task SaveAsync(CompanySetting s)
        {
            await using var db = await _factory.CreateDbContextAsync();
            db.CompanySettings.Update(s);
            await db.SaveChangesAsync();
            Current = s;
            Fmt.Currency = s.Currency;
        }

        public async Task BackupAsync(string filePath)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var dbName = db.Database.GetDbConnection().Database;
            var sql = $"BACKUP DATABASE [{dbName.Replace("]", "]]")}] TO DISK = {{0}} WITH INIT, FORMAT";
            await db.Database.ExecuteSqlRawAsync(sql, filePath);
        }
    }
}
