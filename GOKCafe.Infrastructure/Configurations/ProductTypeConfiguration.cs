using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class ProductTypeConfiguration : IEntityTypeConfiguration<ProductType>
{
    public void Configure(EntityTypeBuilder<ProductType> builder)
    {
        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pt => pt.Description)
            .HasMaxLength(500);

        builder.Property(pt => pt.Slug)
            .IsRequired()
            .HasMaxLength(100);

        // Unique index on slug for URL-friendly identifiers
        builder.HasIndex(pt => pt.Slug)
            .IsUnique();

        // Index for active status filtering
        builder.HasIndex(pt => pt.IsActive);

        // Index for display ordering
        builder.HasIndex(pt => pt.DisplayOrder);

        // Composite index for common query pattern (active + ordering)
        builder.HasIndex(pt => new { pt.IsActive, pt.DisplayOrder });

        // Relationships
        builder.HasMany(pt => pt.Products)
            .WithOne(p => p.ProductType)
            .HasForeignKey(p => p.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(pt => pt.ProductAttributes)
            .WithOne(pa => pa.ProductType)
            .HasForeignKey(pa => pa.ProductTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
