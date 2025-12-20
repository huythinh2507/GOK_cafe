using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class ProductAttributeSelectionConfiguration : IEntityTypeConfiguration<ProductAttributeSelection>
{
    public void Configure(EntityTypeBuilder<ProductAttributeSelection> builder)
    {
        builder.HasKey(pas => pas.Id);

        // Index for ProductId (foreign key)
        builder.HasIndex(pas => pas.ProductId);

        // Index for ProductAttributeId (foreign key)
        builder.HasIndex(pas => pas.ProductAttributeId);

        // Index for ProductAttributeValueId (foreign key)
        builder.HasIndex(pas => pas.ProductAttributeValueId);

        // Unique index to prevent duplicate selections (same product + attribute + value)
        builder.HasIndex(pas => new { pas.ProductId, pas.ProductAttributeId, pas.ProductAttributeValueId })
            .IsUnique();

        // Relationships
        builder.HasOne(pas => pas.Product)
            .WithMany(p => p.ProductAttributeSelections)
            .HasForeignKey(pas => pas.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pas => pas.ProductAttribute)
            .WithMany(pa => pa.ProductAttributeSelections)
            .HasForeignKey(pas => pas.ProductAttributeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pas => pas.ProductAttributeValue)
            .WithMany(pav => pav.ProductAttributeSelections)
            .HasForeignKey(pas => pas.ProductAttributeValueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
