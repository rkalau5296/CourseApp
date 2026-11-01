using CourseApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseApp.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseModule> CourseModules => Set<CourseModule>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(c => c.CourseId);
            entity.Property(c => c.Title).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Section).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<CourseModule>(entity =>
        {
            entity.HasKey(m => m.ModuleId);
            entity.Property(m => m.Title).IsRequired().HasMaxLength(200);
            entity.Property(m => m.SnapshotDescription).HasMaxLength(1000);

            entity.HasOne(m => m.Course)
                  .WithMany(c => c.Modules)
                  .HasForeignKey(m => m.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}