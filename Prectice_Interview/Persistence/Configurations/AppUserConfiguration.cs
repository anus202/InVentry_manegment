using Prectice_Interview.Data;
using Prectice_Interview.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Prectice_Interview.Persistence.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.HasIndex(x => x.Username);
            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
