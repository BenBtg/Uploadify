using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace VoiceVault.Maui.Services
{
    public class UploadService : IUploadService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "http://localhost:5171/api/upload";
        private readonly string _completeUrl = "http://localhost:5171/api/complete";
        private readonly int _chunkSize = 1024 * 1024; // 1 MB
        private double _progress;

        public UploadService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public double Progress
        {
            get => _progress;
            private set
            {
                _progress = value;
                // Notify UI of progress change (if using data binding)
                ProgressChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ProgressChanged;

        public async Task UploadFileAsync(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var totalChunks = (int)Math.Ceiling((double)new FileInfo(filePath).Length / _chunkSize);

            for (int chunkNumber = 1; chunkNumber <= totalChunks; chunkNumber++)
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.Seek((chunkNumber - 1) * _chunkSize, SeekOrigin.Begin);
                    var buffer = new byte[_chunkSize];
                    var bytesRead = await fileStream.ReadAsync(buffer, 0, _chunkSize);

                    using (var content = new MultipartFormDataContent())
                    {
                        var chunkContent = new ByteArrayContent(buffer, 0, bytesRead);
                        chunkContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                        content.Add(chunkContent, "chunk", fileName);
                        content.Add(new StringContent(fileName), "fileName");
                        content.Add(new StringContent(chunkNumber.ToString()), "chunkNumber");

                        var response = await _httpClient.PostAsync(_apiUrl, content);
                        response.EnsureSuccessStatusCode();
                    }
                }

                // Update progress
                Progress = (double)chunkNumber / totalChunks * 100;
            }

            // Notify the server that the upload is complete
            var completeResponse = await _httpClient.PostAsync(_completeUrl, new StringContent(fileName));
            completeResponse.EnsureSuccessStatusCode();
        }
    }
}