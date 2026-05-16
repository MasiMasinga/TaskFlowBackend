using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Models;

namespace TaskFlow.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Token)
            .IsRequired()
            .HasMaxLength(256);
        builder.HasIndex(r => r.Token)
            .IsUnique();
        builder.Property(r => r.ExpiresAtUtc)
            .IsRequired();
        builder.Property(r => r.ReplacedByToken)
            .HasMaxLength(256);
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(r => r.UserId);
    }
}
