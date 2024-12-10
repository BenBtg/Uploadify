using Microsoft.AspNetCore.Mvc;

namespace VoiceVaultAPI.Controllers
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
                    <title>VoiceVault API</title>
                </head>
                <body>
                    <h1>Welcome to VoiceVault API</h1>
                    <p>Use the following endpoints to interact with the API:</p>
                    <ul>
                        <li>POST /api/upload - Upload a file chunk</li>
                        <li>POST /api/complete - Complete the file upload</li>
                    </ul>
                </body>
                </html>";
            return Content(htmlContent, "text/html");
        }
    }
}