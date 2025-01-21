using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VoiceVaultAPI.Controllers
{
    public class UploadSession
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public long BytesUploaded { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    [Route("api")]
    [ApiController]
    public class VoiceVaultController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, UploadSession> UploadSessions = new ConcurrentDictionary<string, UploadSession>();
        private static readonly ConcurrentDictionary<string, MemoryStream> FileStreams = new ConcurrentDictionary<string, MemoryStream>();
        private readonly ILogger<VoiceVaultController> _logger;

        public VoiceVaultController(ILogger<VoiceVaultController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Create an upload session (like OneDrive createUploadSession).
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <param name="fileSize">Total size, in bytes</param>
        [HttpPost("createUploadSession")]
        public IActionResult CreateUploadSession([FromQuery] string fileName, [FromQuery] long fileSize)
        {
            var sessionId = Guid.NewGuid().ToString();
            var session = new UploadSession
            {
                Id = sessionId,
                FileName = fileName,
                FileSize = fileSize,
                BytesUploaded = 0,
                CreatedAt = DateTime.UtcNow
            };

            // Create file stream dictionary entry
            FileStreams[sessionId] = new MemoryStream();
            UploadSessions[sessionId] = session;
            _logger.LogInformation("Created upload session {SessionId} for file {FileName}", sessionId, fileName);

            // Return session info
            var response = new
            {
                uploadUrl = Url.Action(nameof(UploadChunk), "VoiceVault", new { sessionId }, Request.Scheme),
                expirationDateTime = DateTime.UtcNow.AddMinutes(30),
                sessionId
            };
            return Ok(response);
        }

        /// <summary>
        /// Upload a chunk of data to the given session (like OneDrive upload).
        /// </summary>
        /// <param name="sessionId">ID of the upload session</param>
        /// <param name="chunk">Chunk data</param>
        /// <param name="rangeStart">Byte offset for current chunk start</param>
        /// <param name="rangeEnd">Byte offset for current chunk end</param>
        [HttpPut("uploadChunk/{sessionId}")]
        public async Task<IActionResult> UploadChunk(string sessionId, IFormFile chunk, [FromQuery] long rangeStart, [FromQuery] long rangeEnd)
        {
            if (!UploadSessions.TryGetValue(sessionId, out var session))
            {
                _logger.LogWarning("Upload session {SessionId} not found", sessionId);
                return NotFound("Session not found.");
            }
            if (!FileStreams.TryGetValue(sessionId, out var stream))
            {
                _logger.LogWarning("MemoryStream for session {SessionId} not found", sessionId);
                return NotFound("Session not found.");
            }

            if (chunk == null || chunk.Length == 0)
            {
                _logger.LogWarning("Empty chunk received for session {SessionId}", sessionId);
                return BadRequest("No chunk uploaded.");
            }

            _logger.LogInformation("Uploading chunk {Start}-{End} for session {SessionId}", rangeStart, rangeEnd, sessionId);

            using (var tempStream = new MemoryStream())
            {
                await chunk.CopyToAsync(tempStream);
                var chunkData = tempStream.ToArray();

                // Position the main stream at the correct offset
                stream.Seek(rangeStart, SeekOrigin.Begin);
                stream.Write(chunkData, 0, chunkData.Length);
            }

            // Update upload session
            session.BytesUploaded = Math.Max(session.BytesUploaded, rangeEnd + 1);

            // Return status
            return Ok(new
            {
                sessionId,
                nextExpectedRanges = new[] { $"{rangeEnd + 1}-{session.FileSize - 1}" } // Like OneDrive pattern
            });
        }

        /// <summary>
        /// Complete the upload session (like OneDrive finalize).
        /// </summary>
        [HttpPost("completeUpload/{sessionId}")]
        public IActionResult CompleteUpload(string sessionId)
        {
            if (!UploadSessions.TryGetValue(sessionId, out var session) || !FileStreams.TryGetValue(sessionId, out var stream))
            {
                _logger.LogWarning("Session {SessionId} not found or stream missing", sessionId);
                return NotFound("Session not found.");
            }

            if (session.BytesUploaded < session.FileSize)
            {
                _logger.LogWarning("Session {SessionId} incomplete. BytesUploaded={BytesUploaded}, FileSize={FileSize}", sessionId, session.BytesUploaded, session.FileSize);
                return BadRequest("Upload incomplete.");
            }

            _logger.LogInformation("Completing upload session {SessionId} for file {FileName}", sessionId, session.FileName);

            // Here you could save the MemoryStream to disk, database, etc.
            // For demonstration, we just remove it from memory.
            stream.Seek(0, SeekOrigin.Begin);
            var finalData = stream.ToArray();

            // Clean up resources
            UploadSessions.TryRemove(sessionId, out _);
            FileStreams.TryRemove(sessionId, out _);

            return Ok(new { sessionId, session.FileName, session.FileSize, message = "Upload complete" });
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
