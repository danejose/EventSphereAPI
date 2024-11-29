using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace FileUploadApi.Controllers
{
    [ApiController]
    [Route("api/[action]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(IWebHostEnvironment environment, ILogger<FileUploadController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpPost]
        [ActionName("upload")]
        public async Task<IActionResult> UploadVideo([FromForm] IFormFile? file, [FromForm] string? videoUrl)
        {
            try
            {
                // Validate that at least one input is provided (file or URL)
                if ((file == null || file.Length == 0) && string.IsNullOrWhiteSpace(videoUrl))
                {
                    _logger.LogWarning("No file or video URL provided.");
                    return BadRequest("Please provide either a video file or a video URL.");
                }

                // Process the uploaded file
                string? savedFileName = null;
                if (file != null && file.Length > 0)
                {
                    _logger.LogInformation($"File received: {file.FileName}, Size: {file.Length} bytes");

                    // Check file size (500MB limit)
                    if (file.Length > 500 * 1024 * 1024)
                    {
                        return BadRequest("File size exceeds the maximum limit of 500MB.");
                    }

                    // Validate MIME type to allow only video formats
                    var allowedVideoMimeTypes = new[]
                    {
                "video/mp4",
                "video/x-matroska", // .mkv
                "video/quicktime",  // .mov
                "video/x-msvideo",  // .avi
                "video/mpeg"        // .mpeg
            };

                    if (!allowedVideoMimeTypes.Contains(file.ContentType))
                    {
                        return BadRequest("Only video files are allowed.");
                    }

                    // Save the file
                    var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadFolder);

                    savedFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadFolder, savedFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    _logger.LogInformation($"Video uploaded successfully: {savedFileName}");
                }

                // Validate the provided URL
                if (!string.IsNullOrWhiteSpace(videoUrl))
                {
                    if (!Uri.TryCreate(videoUrl, UriKind.Absolute, out var validatedUrl) ||
                        !(validatedUrl.Scheme == Uri.UriSchemeHttp || validatedUrl.Scheme == Uri.UriSchemeHttps))
                    {
                        return BadRequest("The provided video URL is not valid.");
                    }

                    _logger.LogInformation($"Video URL provided: {videoUrl}");
                }

                return Ok(new
                {
                    message = "Video data processed successfully",
                    uploadedFileName = savedFileName,
                    videoUrl = videoUrl,
                    originalFileName = file?.FileName,
                    fileSize = file?.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video processing failed");
                return StatusCode(500, "An error occurred while processing video data");
            }
        }

    }
}
