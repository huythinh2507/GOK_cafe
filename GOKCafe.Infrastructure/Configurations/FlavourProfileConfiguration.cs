using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class FlavourProfileConfiguration : IEntityTypeConfiguration<FlavourProfile>
{
    public void Configure(EntityTypeBuilder<FlavourProfile> builder)
    {
        builder.HasKey(fp => fp.Id);

        builder.Property(fp => fp.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(fp => fp.Description)
            .HasMaxLength(500);

        builder.HasIndex(fp => fp.Name)
            .IsUnique();

        builder.HasIndex(fp => fp.IsActive);
        builder.HasIndex(fp => fp.DisplayOrder);
    }
}
