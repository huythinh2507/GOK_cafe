using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class BlogCategoryConfiguration : IEntityTypeConfiguration<BlogCategory>
{
    public void Configure(EntityTypeBuilder<BlogCategory> builder)
    {
        builder.HasKey(bc => bc.Id);

        builder.Property(bc => bc.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(bc => bc.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(bc => bc.Description)
            .HasMaxLength(500);

        // Unique index for slug
        builder.HasIndex(bc => bc.Slug)
            .IsUnique();

        // Relationship with Blogs
        builder.HasMany(bc => bc.Blogs)
            .WithOne(b => b.Category)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
