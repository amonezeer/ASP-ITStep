using Microsoft.EntityFrameworkCore;
using ASP_ITStep.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ASP_ITStep.Data.Configurations
{
    public class GroupConfiguration : IEntityTypeConfiguration<Entities.ProductGroup>
    {
        public void Configure(EntityTypeBuilder<Entities.ProductGroup> builder)
        {
            builder
                .HasIndex(p => p.Slug)
                .IsUnique();

            builder
                .HasOne(g => g.ParentGroup)
                .WithMany()
                .HasForeignKey(g => g.ParentId);

            builder
                .HasMany(g => g.Products)
                .WithOne(p => p.Group)
                .HasForeignKey(p => p.GroupId);
        }
    }
}
