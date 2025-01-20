using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VoiceVaultAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class VoiceVaultController : ControllerBase
    {
            private static readonly ConcurrentDictionary<string, MemoryStream> FileStreams = new ConcurrentDictionary<string, MemoryStream>();
            private readonly ILogger<VoiceVaultController> _logger;

            public VoiceVaultController(ILogger<VoiceVaultController> logger)
            {
                _logger = logger;
            }

            /// <summary>
            /// Uploads a chunk of a file.
            /// </summary>
            /// <param name="chunk">The file chunk to upload.</param>
            /// <param name="fileName">The name of the file being uploaded.</param>
            /// <param name="chunkNumber">The chunk number of the file.</param>
            /// <returns>An IActionResult indicating the result of the upload.</returns>
            [HttpPost("upload")]
            public async Task<IActionResult> Upload([FromForm] IFormFile chunk, [FromForm] string fileName, [FromForm] int chunkNumber)
            {
                _logger.LogInformation("Received chunk {Number} for file {File}, size: {Size} bytes", chunkNumber, fileName, chunk?.Length ?? 0);

                if (chunk == null || chunk.Length == 0)
                {
                    _logger.LogWarning("No valid chunk received for file {File}", fileName);
                    return BadRequest("No chunk uploaded.");
                }

                var memoryStream = FileStreams.GetOrAdd(fileName, new MemoryStream());

                using (var tempStream = new MemoryStream())
                {
                    await chunk.CopyToAsync(tempStream);
                    var chunkData = tempStream.ToArray();
                    memoryStream.Write(chunkData, 0, chunkData.Length);
                }

                _logger.LogDebug("Chunk {Number} appended to file {File} in memory", chunkNumber, fileName);

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
