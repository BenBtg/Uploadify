using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

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
            private static readonly ConcurrentDictionary<string, MemoryStream> FileStreams = new ConcurrentDictionary<string, MemoryStream>();

            [HttpPost("upload")]
            public async Task<IActionResult> Upload([FromForm] IFormFile chunk, [FromForm] string fileName, [FromForm] int chunkNumber)
            {
                if (chunk == null || chunk.Length == 0)
                    return BadRequest("No chunk uploaded.");

                var memoryStream = FileStreams.GetOrAdd(fileName, new MemoryStream());

                using (var tempStream = new MemoryStream())
                {
                    await chunk.CopyToAsync(tempStream);
                    var chunkData = tempStream.ToArray();
                    memoryStream.Write(chunkData, 0, chunkData.Length);
                }

                return Ok(new { chunkNumber });
            }

            [HttpPost("complete")]
            public IActionResult Complete([FromForm] string fileName)
            {
                if (FileStreams.TryRemove(fileName, out var memoryStream))
                {
                    // Process the complete file in memory if needed
                    // For example, you can save it to a database or perform other operations

                    memoryStream.Dispose();
                    return Ok("File uploaded successfully (simulated).");
                }

                return BadRequest("File not found.");
            }
        }
    }
}
