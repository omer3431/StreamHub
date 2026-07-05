using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamHub.Data;
using StreamHub.Models;

namespace StreamHub.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search, string? category)
    {
        var query = _db.Videos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v => v.Title.Contains(search) || v.Description.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
        {
            query = query.Where(v => v.Category == category);
        }

        ViewBag.Categories = new[] { "All", "Movies", "Series", "Documentary", "Music", "Education", "Other" };
        ViewBag.SelectedCategory = category ?? "All";
        ViewBag.Search = search ?? "";

        var continueWatching = new List<WatchHistory>();
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                continueWatching = await _db.WatchHistories
                    .Include(w => w.Video)
                    .Where(w => w.UserId == userId && w.Video != null)
                    .OrderByDescending(w => w.LastWatchedAt)
                    .Take(6)
                    .ToListAsync();
            }
        }
        ViewBag.ContinueWatching = continueWatching;

        var videos = await query.OrderByDescending(v => v.UploadDate).ToListAsync();
        return View(videos);
    }

    public IActionResult Error()
    {
        return View();
    }
}
