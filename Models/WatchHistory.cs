namespace StreamHub.Models;

public class WatchHistory
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int VideoId { get; set; }
    public Video? Video { get; set; }

    public double LastPositionSeconds { get; set; }
    public DateTime LastWatchedAt { get; set; } = DateTime.UtcNow;
}
