using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ASP_ITStep.Data.Entities;

namespace ASP_ITStep.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Entities.Product>
    {
        public void Configure(EntityTypeBuilder<Entities.Product> builder)
        {
            builder.HasIndex(p =>  p.Slug).IsUnique();
        }
    }
}
