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
        public async Task<IActionResult> UploadFile([FromForm] IFormFile? file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("No file uploaded.");
                    return BadRequest("No file uploaded.");
                }

                _logger.LogInformation($"File received: {file.FileName}, Size: {file.Length} bytes");

                if (file.Length > 10 * 1024 * 1024)
                    return BadRequest("File size exceeds maximum limit of 500MB.");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".mp4"};
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest("File type not allowed.");

                var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                _logger.LogInformation($"File uploaded successfully: {uniqueFileName}");

                return Ok(new
                {
                    message = "File uploaded successfully",
                    fileName = uniqueFileName,
                    originalFileName = file.FileName,
                    fileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File upload failed");
                return StatusCode(500, "An error occurred during file upload");
            }
        }

    }
}
