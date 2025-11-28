using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class ProductEquipmentConfiguration : IEntityTypeConfiguration<ProductEquipment>
{
    public void Configure(EntityTypeBuilder<ProductEquipment> builder)
    {
        builder.HasKey(pe => new { pe.ProductId, pe.EquipmentId });

        builder.HasOne(pe => pe.Product)
            .WithMany(p => p.ProductEquipments)
            .HasForeignKey(pe => pe.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pe => pe.Equipment)
            .WithMany(e => e.ProductEquipments)
            .HasForeignKey(pe => pe.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pe => pe.ProductId);
        builder.HasIndex(pe => pe.EquipmentId);
    }
}
