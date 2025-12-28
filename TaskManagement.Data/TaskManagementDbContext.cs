using Microsoft.EntityFrameworkCore;
using TaskManagement.Data.Entities;

namespace TaskManagement.Data;

/// <summary>
/// Database context for Task Management application
/// </summary>
public class TaskManagementDbContext : DbContext
{
    public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<UserDetails> UserDetails { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure many-to-many relationship between Task and Tag
        modelBuilder.Entity<TaskItem>()
            .HasMany(t => t.Tags)
            .WithMany(tag => tag.Tasks)
            .UsingEntity(j => j.ToTable("TaskTags"));

        // Configure indexes for performance
        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.DueDate)
            .HasDatabaseName("IX_Tasks_DueDate");

        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.UserDetailsId)
            .HasDatabaseName("IX_Tasks_UserDetailsId");

        modelBuilder.Entity<Tag>()
            .HasIndex(t => new { t.Name, t.UserDetailsId })
            .IsUnique()
            .HasDatabaseName("IX_Tags_Name_UserId");

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.UserDetailsId)
            .HasDatabaseName("IX_Tags_UserDetailsId");

        modelBuilder.Entity<UserDetails>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_UserDetails_Email");

        // Configure cascade delete behavior
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.UserDetails)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserDetailsId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tag>()
            .HasOne(t => t.UserDetails)
            .WithMany()
            .HasForeignKey(t => t.UserDetailsId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
