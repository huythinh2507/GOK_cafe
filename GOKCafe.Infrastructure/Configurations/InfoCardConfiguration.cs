using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class InfoCardConfiguration : IEntityTypeConfiguration<InfoCard>
{
    public void Configure(EntityTypeBuilder<InfoCard> builder)
    {
        builder.HasKey(ic => ic.Id);

        builder.Property(ic => ic.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ic => ic.Description)
            .HasMaxLength(1000);

        builder.Property(ic => ic.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ic => ic.ButtonText)
            .HasMaxLength(50);

        builder.Property(ic => ic.ButtonLink)
            .HasMaxLength(500);
    }
}
