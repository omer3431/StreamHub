using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace StreamHub.Models.ViewModels;

public class UploadVideoViewModel
{
    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = "Other";

    [Required(ErrorMessage = "Please choose a video file.")]
    [Display(Name = "Video file")]
    public IFormFile VideoFile { get; set; } = default!;

    [Display(Name = "Thumbnail image (optional)")]
    public IFormFile? ThumbnailFile { get; set; }
}
