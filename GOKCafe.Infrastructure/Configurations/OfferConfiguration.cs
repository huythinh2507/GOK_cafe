using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Description)
            .HasMaxLength(1000);

        builder.Property(o => o.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.DiscountPercentage)
            .HasColumnType("decimal(5,2)");

        builder.Property(o => o.DiscountAmount)
            .HasColumnType("decimal(18,2)");
    }
}
