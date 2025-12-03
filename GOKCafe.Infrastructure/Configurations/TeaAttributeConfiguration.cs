using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class TeaAttributeConfiguration : IEntityTypeConfiguration<TeaAttribute>
{
    public void Configure(EntityTypeBuilder<TeaAttribute> builder)
    {
        builder.HasKey(ta => ta.Id);

        builder.Property(ta => ta.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ta => ta.Description)
            .HasMaxLength(500);

        builder.Property(ta => ta.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);
    }
}
