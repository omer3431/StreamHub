using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StreamHub.Models;

namespace StreamHub.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Video> Videos => Set<Video>();
    public DbSet<WatchHistory> WatchHistories => Set<WatchHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // One watch-history row per (user, video) pair — we update it instead of duplicating.
        builder.Entity<WatchHistory>()
            .HasIndex(w => new { w.UserId, w.VideoId })
            .IsUnique();

        builder.Entity<WatchHistory>()
            .HasOne(w => w.Video)
            .WithMany(v => v.WatchHistories)
            .HasForeignKey(w => w.VideoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
