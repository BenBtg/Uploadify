using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VoiceVaultAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceVaultController : ControllerBase
    {
        [ApiController]
        public class FileUploadController : ControllerBase
        {
            [HttpPost("upload")]
            public async Task<IActionResult> Upload(IFormFile file)
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                // Simulate processing the file in memory
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    // You can add any processing logic here if needed
                }

                return Ok("File uploaded successfully (simulated).");
            }
        }
    }
}
