using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        builder.HasKey(pav => pav.Id);

        builder.Property(pav => pav.Value)
            .IsRequired()
            .HasMaxLength(200);

        // Index for ProductAttributeId (foreign key)
        builder.HasIndex(pav => pav.ProductAttributeId);

        // Index for active status filtering
        builder.HasIndex(pav => pav.IsActive);

        // Index for display ordering
        builder.HasIndex(pav => pav.DisplayOrder);

        // Composite index for common query pattern (attribute + active + ordering)
        builder.HasIndex(pav => new { pav.ProductAttributeId, pav.IsActive, pav.DisplayOrder });

        // Relationships
        builder.HasOne(pav => pav.ProductAttribute)
            .WithMany(pa => pa.AttributeValues)
            .HasForeignKey(pav => pav.ProductAttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(pav => pav.ProductAttributeSelections)
            .WithOne(pas => pas.ProductAttributeValue)
            .HasForeignKey(pas => pas.ProductAttributeValueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
