using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StreamHub.Models;

namespace StreamHub.Data;

public static class DbSeeder
{
    // Runs once on startup. If the Videos table is already populated, it does nothing —
    // so it's always safe to leave this in place, even after real users start uploading.
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (await db.Videos.AnyAsync())
        {
            return; // already seeded (or real videos exist) — don't touch it
        }

        // A system account to "own" the demo videos, so every uploaded video still has
        // a valid uploader without needing a real person to have signed up first.
        const string systemEmail = "team@streamhub.local";
        var systemUser = await userManager.FindByEmailAsync(systemEmail);

        if (systemUser == null)
        {
            systemUser = new ApplicationUser
            {
                UserName = systemEmail,
                Email = systemEmail,
                DisplayName = "StreamHub Team",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(systemUser, "StreamHub#Seed123");
            if (!result.Succeeded)
            {
                // If user creation fails for some reason, bail out rather than seeding
                // videos with no valid owner.
                return;
            }
        }

        var demoVideos = new List<Video>
        {
            new Video
            {
                Title = "Big Buck Bunny",
                Description = "A gentle giant rabbit is bullied by three rodents, until he decides to strike back. A 2008 open-source animated short from the Blender Foundation.",
                Category = "Movies",
                VideoPath = "/videos/big-buck-bunny.mp4",
                ThumbnailPath = "/thumbnails/big-buck-bunny.jpg",
                UploadedById = systemUser.Id,
                UploadDate = DateTime.UtcNow.AddDays(-14),
                Views = 128
            },
            new Video
            {
                Title = "Sintel",
                Description = "A lonely girl named Sintel searches for a baby dragon she raised and lost. A 2012 open-source fantasy short from the Blender Foundation.",
                Category = "Movies",
                VideoPath = "/videos/sintel.mp4",
                ThumbnailPath = "/thumbnails/sintel.jpg",
                UploadedById = systemUser.Id,
                UploadDate = DateTime.UtcNow.AddDays(-10),
                Views = 94
            },
              new Video
            {
                Title = "Game play",
                Description = "wuthering Waves new Character.",
                Category = "Movies",
                VideoPath = "/videos/game play.mp4",
                ThumbnailPath = "/thumbnails/suisui.jpg",
                UploadedById = systemUser.Id,
                UploadDate = DateTime.UtcNow.AddDays(-10),
                Views = 94
            },
            new Video
            {
                Title = "Jellyfish",
                Description = "A calming close-up look at jellyfish drifting through the water.",
                Category = "Documentary",
                VideoPath = "/videos/jellyfish.mp4",
                ThumbnailPath = "/thumbnails/jellyfish.jpg",
                UploadedById = systemUser.Id,
                UploadDate = DateTime.UtcNow.AddDays(-7),
                Views = 76
            }
        };

        db.Videos.AddRange(demoVideos);
        await db.SaveChangesAsync();
    }
}