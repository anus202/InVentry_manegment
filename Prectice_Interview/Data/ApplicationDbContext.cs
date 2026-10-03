using Microsoft.EntityFrameworkCore;

namespace Prectice_Interview.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Login> Logins { get; set; }

    }
}