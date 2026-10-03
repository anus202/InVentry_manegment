using Prectice_Interview.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Prectice_Interview.Persistence
{
    public class DesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=.;Database=PrecticeInterviewDb;Trusted_Connection=True;TrustServerCertificate=True")
                .Options;
            return new ApplicationDbContext(options);
        }
    }
}
