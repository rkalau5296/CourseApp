using CourseApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseApp.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<CourseNode> CourseNodes => Set<CourseNode>();
    public DbSet<CourseItem> CourseItems => Set<CourseItem>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CourseNode>(entity =>
        {
            entity.HasKey(n => n.CourseNodeId);
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Description).HasMaxLength(1000);

            entity.HasOne(n => n.Parent)
                  .WithMany(n => n.Children)
                  .HasForeignKey(n => n.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CourseItem>(entity =>
        {
            entity.HasKey(i => i.CourseItemId);
            entity.Property(i => i.Title).IsRequired().HasMaxLength(200);
            entity.Property(i => i.FilePath).HasMaxLength(500);
            entity.Property(i => i.Type).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(i => i.CourseNode)
                  .WithMany(n => n.Items)
                  .HasForeignKey(i => i.CourseNodeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
