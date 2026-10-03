using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prectice_Interview.Data;

namespace Prectice_Interview.Services
{
    public static class App
    {
        public static IServiceProvider Services { get; set; } = null!;
        public static T Get<T>() where T : notnull => Services.GetRequiredService<T>();
        public static ApplicationDbContext NewDb() => Get<IDbContextFactory<ApplicationDbContext>>().CreateDbContext();
    }
}
