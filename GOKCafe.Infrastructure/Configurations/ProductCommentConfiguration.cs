using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class ProductCommentConfiguration : IEntityTypeConfiguration<ProductComment>
{
    public void Configure(EntityTypeBuilder<ProductComment> builder)
    {
        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.Comment)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(pc => pc.Rating)
            .IsRequired();

        builder.Property(pc => pc.IsApproved)
            .HasDefaultValue(false);

        // Index for product filtering
        builder.HasIndex(pc => pc.ProductId);

        // Index for user filtering
        builder.HasIndex(pc => pc.UserId);

        // Index for approved comments
        builder.HasIndex(pc => pc.IsApproved);

        // Composite index for common query pattern (product + approved)
        builder.HasIndex(pc => new { pc.ProductId, pc.IsApproved });

        // Relationship with Product
        builder.HasOne(pc => pc.Product)
            .WithMany(p => p.ProductComments)
            .HasForeignKey(pc => pc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with User
        builder.HasOne(pc => pc.User)
            .WithMany(u => u.ProductComments)
            .HasForeignKey(pc => pc.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing relationship for replies
        builder.HasOne(pc => pc.ParentComment)
            .WithMany(pc => pc.Replies)
            .HasForeignKey(pc => pc.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
