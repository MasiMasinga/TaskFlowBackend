using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Models;

namespace TaskFlow.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(t => t.Description)
            .HasMaxLength(4000);
        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(t => t.CreatedAtUtc)
            .IsRequired();
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.ProjectId);
    }
}