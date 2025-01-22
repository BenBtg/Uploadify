using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UploadifyAPI.Controllers
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
    public class UploadifyController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, UploadSession> UploadSessions = new ConcurrentDictionary<string, UploadSession>();
        private static readonly ConcurrentDictionary<string, MemoryStream> FileStreams = new ConcurrentDictionary<string, MemoryStream>();
        private readonly ILogger<UploadifyController> _logger;

        public UploadifyController(ILogger<UploadifyController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Create an upload session (like OneDrive createUploadSession).
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <param name="fileSize">Total size, in bytes</param>
        [HttpPost("createUploadSession")]
        public IActionResult CreateUploadSession([FromForm] string fileName, [FromForm] long fileSize)
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
                uploadUrl = Url.Action(nameof(UploadChunk), "Uploadify", new { sessionId }, Request.Scheme),
                expirationDateTime = DateTime.UtcNow.AddMinutes(30),
                sessionId
            };
            return Ok(response);
        }

        [HttpGet("upload/{sessionId}/nextrange")]
        public IActionResult GetNextExpectedRange(string sessionId)
        {
            if (!UploadSessions.TryGetValue(sessionId, out var session))
            {
                return NotFound("Upload session not found.");
            }

            return Ok(new { NextExpectedRangeStart = session.BytesUploaded });
        }

        [HttpPost("upload/{sessionId}/chunk")]
        public async Task<IActionResult> UploadChunk(string sessionId, [FromForm] IFormFile chunk, [FromForm] int chunkNumber)
        {
            _logger.LogInformation("Received chunk {Number} for session {SessionId}, size: {Size} bytes", chunkNumber, sessionId, chunk?.Length ?? 0);

            if (chunk == null || chunk.Length == 0)
            {
                return BadRequest("Invalid chunk.");
            }

            if (!UploadSessions.TryGetValue(sessionId, out var session))
            {
                return NotFound("Upload session not found.");
            }

            using (var stream = new MemoryStream())
            {
                await chunk.CopyToAsync(stream);
                session.BytesUploaded += stream.Length;
                FileStreams[sessionId].Write(stream.ToArray(), 0, (int)stream.Length);
            }

            return Ok(new { sessionId, chunkNumber, message = "Chunk uploaded successfully." });
        }

        [HttpPost("upload/{sessionId}/complete")]
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
    }
}
