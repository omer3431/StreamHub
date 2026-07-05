using System.ComponentModel.DataAnnotations;

namespace StreamHub.Models;

public class Video
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    // Relative path under wwwroot, e.g. "/videos/xxxx.mp4"
    [Required]
    public string VideoPath { get; set; } = string.Empty;

    // Relative path under wwwroot, e.g. "/thumbnails/xxxx.jpg"
    public string? ThumbnailPath { get; set; }

    [StringLength(50)]
    public string Category { get; set; } = "Other";

    public string UploadedById { get; set; } = string.Empty;
    public ApplicationUser? UploadedBy { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public int Views { get; set; } = 0;

    public ICollection<WatchHistory> WatchHistories { get; set; } = new List<WatchHistory>();
}
