using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class ContactInfoConfiguration : IEntityTypeConfiguration<ContactInfo>
{
    public void Configure(EntityTypeBuilder<ContactInfo> builder)
    {
        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ci => ci.Subtitle)
            .HasMaxLength(200);

        builder.Property(ci => ci.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(ci => ci.Address)
            .HasMaxLength(500);

        builder.Property(ci => ci.Phone)
            .HasMaxLength(50);

        builder.Property(ci => ci.Email)
            .HasMaxLength(200);

        builder.Property(ci => ci.WorkingHours)
            .HasMaxLength(200);

        builder.Property(ci => ci.ImageUrl)
            .HasMaxLength(500);

        builder.Property(ci => ci.MapUrl)
            .HasMaxLength(500);

        builder.Property(ci => ci.ButtonText)
            .HasMaxLength(50);

        builder.Property(ci => ci.ButtonLink)
            .HasMaxLength(500);
    }
}
