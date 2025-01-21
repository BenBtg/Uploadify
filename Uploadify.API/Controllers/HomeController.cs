using Microsoft.AspNetCore.Mvc;

namespace UploadifyAPI.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var htmlContent = @"
                <html>
                <head>
                    <title>Uploadify API</title>
                    <style>
                        body { font-family: Arial, sans-serif; margin: 40px; }
                        h1 { color: #333; }
                        ul { list-style-type: none; padding: 0; }
                        li { margin-bottom: 10px; }
                        code { background-color: #f4f4f4; padding: 2px 4px; border-radius: 4px; }
                    </style>
                </head>
                <body>
                    <h1>Welcome to Uploadify API</h1>
                    <p>Use the following endpoints to interact with the API:</p>
                    <ul>
                        <li><strong>POST /api/createUploadSession</strong> - Create an upload session</li>
                        <li><strong>PUT /api/uploadChunk/{sessionId}</strong> - Upload a file chunk</li>
                        <li><strong>GET /api/uploadChunk/{sessionId}/nextRange</strong> - Get the next expected range for the upload session</li>
                        <li><strong>POST /api/completeUpload/{sessionId}</strong> - Complete the file upload</li>
                    </ul>
                    <h2>Endpoint Details</h2>
                    <h3>POST /api/createUploadSession</h3>
                    <p>Creates a new upload session.</p>
                    <p><strong>Parameters:</strong></p>
                    <ul>
                        <li><code>fileName</code> (query) - The name of the file to be uploaded.</li>
                        <li><code>fileSize</code> (query) - The total size of the file in bytes.</li>
                    </ul>
                    <p><strong>Response:</strong></p>
                    <ul>
                        <li><code>uploadUrl</code> - The URL to upload file chunks.</li>
                        <li><code>expirationDateTime</code> - The expiration time of the upload session.</li>
                        <li><code>sessionId</code> - The ID of the upload session.</li>
                    </ul>

                    <h3>PUT /api/uploadChunk/{sessionId}</h3>
                    <p>Uploads a chunk of the file to the given session.</p>
                    <p><strong>Parameters:</strong></p>
                    <ul>
                        <li><code>sessionId</code> (path) - The ID of the upload session.</li>
                        <li><code>chunk</code> (form) - The file chunk to be uploaded.</li>
                        <li><code>rangeStart</code> (query) - The byte offset for the current chunk start.</li>
                        <li><code>rangeEnd</code> (query) - The byte offset for the current chunk end.</li>
                    </ul>
                    <p><strong>Response:</strong></p>
                    <ul>
                        <li><code>sessionId</code> - The ID of the upload session.</li>
                        <li><code>nextExpectedRanges</code> - The next expected byte ranges for the upload session.</li>
                    </ul>

                    <h3>GET /api/uploadChunk/{sessionId}/nextRange</h3>
                    <p>Gets the next expected range for the upload session.</p>
                    <p><strong>Parameters:</strong></p>
                    <ul>
                        <li><code>sessionId</code> (path) - The ID of the upload session.</li>
                    </ul>
                    <p><strong>Response:</strong></p>
                    <ul>
                        <li><code>NextExpectedRangeStart</code> - The byte offset for the next expected range start.</li>
                    </ul>

                    <h3>POST /api/completeUpload/{sessionId}</h3>
                    <p>Completes the file upload session.</p>
                    <p><strong>Parameters:</strong></p>
                    <ul>
                        <li><code>sessionId</code> (path) - The ID of the upload session.</li>
                    </ul>
                    <p><strong>Response:</strong></p>
                    <ul>
                        <li><code>sessionId</code> - The ID of the upload session.</li>
                        <li><code>fileName</code> - The name of the uploaded file.</li>
                        <li><code>fileSize</code> - The size of the uploaded file in bytes.</li>
                        <li><code>message</code> - A message indicating the upload is complete.</li>
                    </ul>
                </body>
                </html>";
            return Content(htmlContent, "text/html");
        }
    }
}