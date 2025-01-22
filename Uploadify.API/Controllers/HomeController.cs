using Microsoft.AspNetCore.Mvc;

namespace Uploadify.API.Controllers
{
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            var htmlContent = @"
                <html>
                <head>
                    <title>Uploadify API Documentation</title>
                </head>
                <body>
                    <h1>Uploadify API Documentation</h1>
                    
                    <h2>Endpoints</h2>
                    
                    <h3>POST /api/createUploadSession</h3>
                    <p>Creates a new upload session.</p>
                    <p><strong>Parameters:</strong></p>
                    <ul>
                        <li><code>fileName</code> (form) - The name of the file to be uploaded.</li>
                        <li><code>fileSize</code> (form) - The size of the file to be uploaded in bytes.</li>
                    </ul>
                    <p><strong>Response:</strong></p>
                    <ul>
                        <li><code>sessionId</code> - The ID of the created upload session.</li>
                        <li><code>uploadUrl</code> - The URL to upload file chunks.</li>
                    </ul>
                    
                    <h3>POST /api/upload/{sessionId}/chunk</h3>
                    <p>Uploads a chunk of the file.</p>
                    <p><strong>Parameters:</strong></p>
                    <ul>
                        <li><code>sessionId</code> (path) - The ID of the upload session.</li>
                        <li><code>chunk</code> (form) - The file chunk to be uploaded.</li>
                        <li><code>chunkNumber</code> (form) - The number of the chunk being uploaded.</li>
                    </ul>
                    <p><strong>Response:</strong></p>
                    <ul>
                        <li><code>sessionId</code> - The ID of the upload session.</li>
                        <li><code>chunkNumber</code> - The number of the uploaded chunk.</li>
                        <li><code>message</code> - A message indicating the chunk upload is successful.</li>
                    </ul>
                    
                    <h3>GET /api/upload/{sessionId}/nextrange</h3>
                    <p>Gets the next expected byte range for the upload.</p>
                    <p><strong>Parameters:</strong></p>
                    <ul>
                        <li><code>sessionId</code> (path) - The ID of the upload session.</li>
                    </ul>
                    <p><strong>Response:</strong></p>
                    <ul>
                        <li><code>NextExpectedRangeStart</code> - The byte offset for the next expected range start.</li>
                    </ul>
                    
                    <h3>POST /api/upload/{sessionId}/complete</h3>
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