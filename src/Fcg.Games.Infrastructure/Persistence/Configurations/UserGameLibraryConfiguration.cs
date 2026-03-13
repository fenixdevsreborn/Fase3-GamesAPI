using Fcg.Games.Domain.Entities;
using Fcg.Games.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Games.Infrastructure.Persistence.Configurations;

public class UserGameLibraryConfiguration : IEntityTypeConfiguration<UserGameLibrary>
{
    public void Configure(EntityTypeBuilder<UserGameLibrary> builder)
    {
        builder.ToTable("user_game_libraries");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.GameId).HasColumnName("game_id").IsRequired();
        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(e => e.SourcePaymentId).HasColumnName("source_payment_id").HasMaxLength(100);
        builder.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(2000);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(e => new { e.UserId, e.GameId }).IsUnique();
        builder.HasOne(e => e.Game).WithMany().HasForeignKey(e => e.GameId).OnDelete(DeleteBehavior.Restrict);
    }
}
