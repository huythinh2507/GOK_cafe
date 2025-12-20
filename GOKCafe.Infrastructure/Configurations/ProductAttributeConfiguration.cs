using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pa => pa.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pa => pa.Description)
            .HasMaxLength(500);

        // Index for ProductTypeId (foreign key)
        builder.HasIndex(pa => pa.ProductTypeId);

        // Index for active status filtering
        builder.HasIndex(pa => pa.IsActive);

        // Index for display ordering
        builder.HasIndex(pa => pa.DisplayOrder);

        // Composite index for common query pattern (type + active + ordering)
        builder.HasIndex(pa => new { pa.ProductTypeId, pa.IsActive, pa.DisplayOrder });

        // Relationships
        builder.HasOne(pa => pa.ProductType)
            .WithMany(pt => pt.ProductAttributes)
            .HasForeignKey(pa => pa.ProductTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(pa => pa.AttributeValues)
            .WithOne(pav => pav.ProductAttribute)
            .HasForeignKey(pav => pav.ProductAttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(pa => pa.ProductAttributeSelections)
            .WithOne(pas => pas.ProductAttribute)
            .HasForeignKey(pas => pas.ProductAttributeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
