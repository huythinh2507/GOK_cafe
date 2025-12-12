using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        // Index for category filtering
        builder.HasIndex(p => p.CategoryId);

        // Index for active status filtering
        builder.HasIndex(p => p.IsActive);

        // Index for featured products
        builder.HasIndex(p => p.IsFeatured);

        // Composite index for common query pattern (category + active + ordering)
        builder.HasIndex(p => new { p.CategoryId, p.IsActive, p.DisplayOrder });

        // Index for display ordering
        builder.HasIndex(p => p.DisplayOrder);

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.DiscountPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);

        builder.HasMany(p => p.ProductImages)
            .WithOne(pi => pi.Product)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
