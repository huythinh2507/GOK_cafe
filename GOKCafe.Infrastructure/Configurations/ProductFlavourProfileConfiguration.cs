using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class ProductFlavourProfileConfiguration : IEntityTypeConfiguration<ProductFlavourProfile>
{
    public void Configure(EntityTypeBuilder<ProductFlavourProfile> builder)
    {
        builder.HasKey(pfp => new { pfp.ProductId, pfp.FlavourProfileId });

        builder.HasOne(pfp => pfp.Product)
            .WithMany(p => p.ProductFlavourProfiles)
            .HasForeignKey(pfp => pfp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pfp => pfp.FlavourProfile)
            .WithMany(fp => fp.ProductFlavourProfiles)
            .HasForeignKey(pfp => pfp.FlavourProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pfp => pfp.ProductId);
        builder.HasIndex(pfp => pfp.FlavourProfileId);
    }
}
