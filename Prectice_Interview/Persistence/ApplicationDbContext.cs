using Prectice_Interview.Data;
using Microsoft.EntityFrameworkCore;
using Prectice_Interview.Services;

namespace Prectice_Interview.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<CompanySetting> CompanySettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            foreach (var property in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(t => t.GetProperties())
                         .Where(p => p.ClrType == typeof(decimal)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAudit();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ApplyAudit();
            return base.SaveChanges();
        }

        private void ApplyAudit()
        {
            var now = DateTime.Now;
            int? uid = Session.UserId;
            foreach (var e in ChangeTracker.Entries<CommonField>())
            {
                if (e.State == EntityState.Added)
                {
                    e.Entity.CreateDate = now;
                    e.Entity.UpdateDate = now;
                    e.Entity.CreateBy = uid;
                }
                else if (e.State == EntityState.Modified)
                {
                    e.Property(x => x.CreateDate).IsModified = false;
                    e.Property(x => x.CreateBy).IsModified = false;
                    e.Entity.UpdateDate = now;
                    e.Entity.UpdateBy = uid;
                }
            }
        }
    }
}
