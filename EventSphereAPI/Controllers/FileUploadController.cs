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
        public async Task<IActionResult> UploadVideo([FromForm] IFormFile? file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("No file uploaded.");
                    return BadRequest("No file uploaded.");
                }

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

                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                _logger.LogInformation($"Video uploaded successfully: {uniqueFileName}");

                return Ok(new
                {
                    message = "Video uploaded successfully",
                    fileName = uniqueFileName,
                    originalFileName = file.FileName,
                    fileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video upload failed");
                return StatusCode(500, "An error occurred during video upload");
            }
        }
    }
}
