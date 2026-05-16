using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Models;

namespace TaskFlow.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(p => p.Description)
            .HasMaxLength(2000);
        builder.Property(p => p.CreatedAtUtc)
            .IsRequired();
        builder.HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(p => p.Owner)
        .WithMany()
        .HasForeignKey(p => p.OwnerId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.OwnerId);
    }
}