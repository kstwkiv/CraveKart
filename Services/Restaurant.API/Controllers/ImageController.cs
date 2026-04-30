using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Restaurant.API.Controllers;

/// <summary>
/// API controller for handling image uploads for restaurant and menu item assets.
/// Stores uploaded images in the wwwroot/uploads directory and returns a public URL.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ImageController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// Initializes a new instance of <see cref="ImageController"/>.
    /// </summary>
    /// <param name="env">The web host environment for resolving the web root path.</param>
    public ImageController(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Uploads an image file and returns its public URL.
    /// Accepts jpg, jpeg, png, and webp formats up to 5MB.
    /// </summary>
    /// <param name="file">The image file to upload.</param>
    /// <returns>The public URL of the uploaded image, or a 400 error on validation failure.</returns>
    [HttpPost("upload")]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest("Only jpg, png, webp images are allowed.");

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("File size must be under 5MB.");

        var uploadsPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var url = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
        return Ok(new { url });
    }
}
