using Prectice_Interview.Data;
using Prectice_Interview.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Prectice_Interview.Persistence.Configurations
{
    public class SaleConfiguration : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            builder.HasIndex(x => x.InvoiceNo);
            builder.HasIndex(x => x.SaleDate);
            builder.HasMany(s => s.Items).WithOne().HasForeignKey(i => i.SaleId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(s => s.Customer).WithMany().HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
