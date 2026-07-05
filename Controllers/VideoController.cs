using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamHub.Data;
using StreamHub.Models;
using StreamHub.Models.ViewModels;

namespace StreamHub.Controllers;

public class VideoController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<ApplicationUser> _userManager;

    private static readonly string[] AllowedVideoExtensions = { ".mp4", ".webm", ".ogg" };
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxVideoSizeBytes = 300L * 1024 * 1024; // 300 MB cap — fine for a demo

    public VideoController(AppDbContext db, IWebHostEnvironment env, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _env = env;
        _userManager = userManager;
    }

    public async Task<IActionResult> Details(int id)
    {
        var video = await _db.Videos.Include(v => v.UploadedBy).FirstOrDefaultAsync(v => v.Id == id);
        if (video == null) return NotFound();

        video.Views++;
        await _db.SaveChangesAsync();

        double startPosition = 0;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            var history = await _db.WatchHistories.FirstOrDefaultAsync(w => w.UserId == userId && w.VideoId == id);
            if (history != null) startPosition = history.LastPositionSeconds;
        }
        ViewBag.StartPosition = startPosition;

        return View(video);
    }

    // Called periodically by the player's JavaScript to remember playback position.
    [HttpPost]
    public async Task<IActionResult> SaveProgress(int videoId, double position)
    {
        if (User.Identity?.IsAuthenticated != true) return Ok();

        var userId = _userManager.GetUserId(User)!;
        var history = await _db.WatchHistories.FirstOrDefaultAsync(w => w.UserId == userId && w.VideoId == videoId);

        if (history == null)
        {
            history = new WatchHistory
            {
                UserId = userId,
                VideoId = videoId,
                LastPositionSeconds = position,
                LastWatchedAt = DateTime.UtcNow
            };
            _db.WatchHistories.Add(history);
        }
        else
        {
            history.LastPositionSeconds = position;
            history.LastWatchedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }

    [Authorize]
    [HttpGet]
    public IActionResult Upload() => View(new UploadVideoViewModel());

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxVideoSizeBytes)]
    public async Task<IActionResult> Upload(UploadVideoViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var videoExt = Path.GetExtension(model.VideoFile.FileName).ToLowerInvariant();
        if (!AllowedVideoExtensions.Contains(videoExt))
        {
            ModelState.AddModelError(nameof(model.VideoFile), "Only .mp4, .webm, or .ogg files are allowed.");
            return View(model);
        }

        if (model.VideoFile.Length > MaxVideoSizeBytes)
        {
            ModelState.AddModelError(nameof(model.VideoFile), "Video file is too large (max 300 MB for this demo).");
            return View(model);
        }

        var videosFolder = Path.Combine(_env.WebRootPath, "videos");
        Directory.CreateDirectory(videosFolder);

        var videoFileName = $"{Guid.NewGuid()}{videoExt}";
        var videoFullPath = Path.Combine(videosFolder, videoFileName);
        using (var stream = new FileStream(videoFullPath, FileMode.Create))
        {
            await model.VideoFile.CopyToAsync(stream);
        }

        string? thumbnailRelativePath = null;
        if (model.ThumbnailFile != null && model.ThumbnailFile.Length > 0)
        {
            var thumbExt = Path.GetExtension(model.ThumbnailFile.FileName).ToLowerInvariant();
            if (AllowedImageExtensions.Contains(thumbExt))
            {
                var thumbsFolder = Path.Combine(_env.WebRootPath, "thumbnails");
                Directory.CreateDirectory(thumbsFolder);
                var thumbFileName = $"{Guid.NewGuid()}{thumbExt}";
                var thumbFullPath = Path.Combine(thumbsFolder, thumbFileName);
                using var thumbStream = new FileStream(thumbFullPath, FileMode.Create);
                await model.ThumbnailFile.CopyToAsync(thumbStream);
                thumbnailRelativePath = $"/thumbnails/{thumbFileName}";
            }
        }

        var video = new Video
        {
            Title = model.Title,
            Description = model.Description,
            Category = model.Category,
            VideoPath = $"/videos/{videoFileName}",
            ThumbnailPath = thumbnailRelativePath,
            UploadedById = _userManager.GetUserId(User)!,
            UploadDate = DateTime.UtcNow
        };

        _db.Videos.Add(video);
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", new { id = video.Id });
    }
}
