using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace VoiceVault.Maui.Services
{
    public class UploadService : IUploadService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _completeUrl;
        private readonly int _chunkSize = 1024 * 1024; // 1 MB
        private double _progress;

        // New fields
        private long _fileSize;
        private Stopwatch _stopwatch;
        private long _bytesUploaded;

        public UploadService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Determine the API base URL based on the platform
        #if ANDROID
            var baseAddress = "http://10.0.2.2:5171/";
        #else
            var baseAddress = "http://localhost:5171/";
        #endif

            _httpClient.BaseAddress = new Uri(baseAddress);
            _apiUrl = "api/upload";
            _completeUrl = "api/complete";
        }

        public double Progress
        {
            get => _progress;
            private set
            {
                if (Math.Abs(_progress - value) > 0.01)
                {
                    _progress = value;
                    ProgressChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler ProgressChanged;

        // New properties
        public long FileSize => _fileSize;
        public DateTime StartTime { get; private set; }
        public TimeSpan ElapsedTime => _stopwatch?.Elapsed ?? TimeSpan.Zero;
        public TimeSpan EstimatedTimeToCompletion
        {
            get
            {
                if (_bytesUploaded == 0 || _stopwatch.ElapsedMilliseconds == 0)
                    return TimeSpan.Zero;

                var totalSeconds = (_fileSize * ElapsedTime.TotalSeconds) / _bytesUploaded;
                var remainingSeconds = totalSeconds - ElapsedTime.TotalSeconds;
                return TimeSpan.FromSeconds(remainingSeconds);
            }
        }

        public async Task UploadFileAsync(Stream fileStream, string fileName)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name must be provided.", nameof(fileName));

            _fileSize = fileStream.Length;
            _bytesUploaded = 0;
            StartTime = DateTime.Now;
            _stopwatch = Stopwatch.StartNew();

            var totalChunks = (int)Math.Ceiling((double)_fileSize / _chunkSize);
            var buffer = new byte[_chunkSize];

            for (int chunkNumber = 1; chunkNumber <= totalChunks; chunkNumber++)
            {
                var bytesRead = await fileStream.ReadAsync(buffer, 0, _chunkSize);

                using var content = new MultipartFormDataContent();
                using var chunkContent = new ByteArrayContent(buffer, 0, bytesRead);
                chunkContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

                content.Add(chunkContent, "chunk", fileName);
                content.Add(new StringContent(fileName), "fileName");
                content.Add(new StringContent(chunkNumber.ToString()), "chunkNumber");

                try
                {
                    var response = await _httpClient.PostAsync(_apiUrl, content);
                    response.EnsureSuccessStatusCode();
                }
                 catch (Exception ex)
                {
                    Debug.WriteLine($"Upload failed: {ex.Message}");
                    // Handle exception (e.g., notify user, retry logic)
                }

                _bytesUploaded += bytesRead;

                // Update progress
                Progress = ((double)_bytesUploaded / _fileSize);
                ProgressChanged?.Invoke(this, EventArgs.Empty);

            }

            _stopwatch.Stop();

            // Notify the server that the upload is complete
            var completeResponse = await _httpClient.PostAsync(_completeUrl, new StringContent(fileName));
            completeResponse.EnsureSuccessStatusCode();
        }
    }
}