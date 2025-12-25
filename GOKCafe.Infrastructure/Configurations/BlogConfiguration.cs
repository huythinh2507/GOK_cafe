using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOKCafe.Infrastructure.Configurations;

public class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Content)
            .IsRequired();

        builder.Property(b => b.Excerpt)
            .HasMaxLength(500);

        builder.Property(b => b.FeaturedImageUrl)
            .HasMaxLength(500);

        builder.Property(b => b.MetaTitle)
            .HasMaxLength(200);

        builder.Property(b => b.MetaDescription)
            .HasMaxLength(500);

        builder.Property(b => b.Tags)
            .HasMaxLength(500);

        builder.Property(b => b.IsPublished)
            .HasDefaultValue(false);

        builder.Property(b => b.ViewCount)
            .HasDefaultValue(0);

        // Unique index for slug
        builder.HasIndex(b => b.Slug)
            .IsUnique();

        // Index for author filtering
        builder.HasIndex(b => b.AuthorId);

        // Index for category filtering
        builder.HasIndex(b => b.CategoryId);

        // Index for published status
        builder.HasIndex(b => b.IsPublished);

        // Composite index for common query pattern (published + publishedAt)
        builder.HasIndex(b => new { b.IsPublished, b.PublishedAt });

        // Index for search by tags
        builder.HasIndex(b => b.Tags);

        // Relationship with User (Author)
        builder.HasOne(b => b.Author)
            .WithMany()
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with BlogCategory
        builder.HasOne(b => b.Category)
            .WithMany(bc => bc.Blogs)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relationship with BlogComments
        builder.HasMany(b => b.Comments)
            .WithOne(bc => bc.Blog)
            .HasForeignKey(bc => bc.BlogId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
