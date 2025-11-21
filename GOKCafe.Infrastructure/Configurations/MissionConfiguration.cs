using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class MissionConfiguration : IEntityTypeConfiguration<Mission>
{
    public void Configure(EntityTypeBuilder<Mission> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Subtitle)
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.MediaUrl)
            .HasMaxLength(500);

        builder.Property(m => m.MediaType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(m => m.ButtonText)
            .HasMaxLength(50);

        builder.Property(m => m.ButtonLink)
            .HasMaxLength(500);
    }
}
