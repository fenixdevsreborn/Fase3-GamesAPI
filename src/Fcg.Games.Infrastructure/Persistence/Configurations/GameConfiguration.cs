using Fcg.Games.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Fcg.Games.Infrastructure.Persistence.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("games");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
        builder.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(350).IsRequired();
        builder.HasIndex(e => e.Slug).IsUnique();

        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(e => e.Genre).HasColumnName("genre").HasMaxLength(100);
        builder.Property(e => e.Studio).HasColumnName("studio").HasMaxLength(200);
        builder.Property(e => e.Price).HasColumnName("price").HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.CoverUrl).HasColumnName("cover_url").HasMaxLength(2000);

        builder.Property(e => e.Tags)
            .HasColumnName("tags")
            .HasConversion(
                v => JsonSerializer.Serialize(v),
                v => JsonSerializer.Deserialize<List<string>>(v) ?? new List<string>())
            .HasMaxLength(2000);

        builder.Property(e => e.IsPublished).HasColumnName("is_published").IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}
