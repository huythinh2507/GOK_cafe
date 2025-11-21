using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class ServiceFeatureConfiguration : IEntityTypeConfiguration<ServiceFeature>
{
    public void Configure(EntityTypeBuilder<ServiceFeature> builder)
    {
        builder.HasKey(sf => sf.Id);

        builder.Property(sf => sf.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sf => sf.Description)
            .HasMaxLength(500);

        builder.Property(sf => sf.IconUrl)
            .IsRequired()
            .HasMaxLength(500);
    }
}
