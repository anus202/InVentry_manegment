using Prectice_Interview.Data;
using Prectice_Interview.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Prectice_Interview.Persistence.Configurations
{
    public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            builder.HasMany(p => p.Items).WithOne().HasForeignKey(i => i.PurchaseId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(p => p.Supplier).WithMany().HasForeignKey(p => p.SupplierId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
