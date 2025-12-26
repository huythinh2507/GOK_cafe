using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class BlogCommentConfiguration : IEntityTypeConfiguration<BlogComment>
{
    public void Configure(EntityTypeBuilder<BlogComment> builder)
    {
        builder.HasKey(bc => bc.Id);

        builder.Property(bc => bc.Comment)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(bc => bc.IsApproved)
            .HasDefaultValue(false);

        // Index for blog filtering
        builder.HasIndex(bc => bc.BlogId);

        // Index for user filtering
        builder.HasIndex(bc => bc.UserId);

        // Index for approved comments
        builder.HasIndex(bc => bc.IsApproved);

        // Composite index for common query pattern (blog + approved)
        builder.HasIndex(bc => new { bc.BlogId, bc.IsApproved });

        // Relationship with Blog
        builder.HasOne(bc => bc.Blog)
            .WithMany(b => b.Comments)
            .HasForeignKey(bc => bc.BlogId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with User
        builder.HasOne(bc => bc.User)
            .WithMany()
            .HasForeignKey(bc => bc.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing relationship for replies
        builder.HasOne(bc => bc.ParentComment)
            .WithMany(bc => bc.Replies)
            .HasForeignKey(bc => bc.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
