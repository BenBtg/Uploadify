using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

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
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isPaused;

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
            _cancellationTokenSource = new CancellationTokenSource();
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

        public async Task UploadFileAsync(string filePath)
        {
            _fileSize = new FileInfo(filePath).Length;
            _stopwatch = Stopwatch.StartNew();
            _bytesUploaded = LoadUploadedBytes(filePath);
            _isPaused = false;

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.Seek(_bytesUploaded, SeekOrigin.Begin);

                while (_bytesUploaded < _fileSize)
                {
                    if (_isPaused)
                    {
                        await Task.Delay(100); // Wait for resume
                        continue;
                    }

                    var buffer = new byte[_chunkSize];
                    var bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);

                    if (bytesRead == 0)
                        break;

                    var content = new MultipartFormDataContent();
                    content.Add(new ByteArrayContent(buffer, 0, bytesRead), "file", Path.GetFileName(filePath));
                    var response = await _httpClient.PostAsync(_apiUrl, content, _cancellationTokenSource.Token);

                    if (!response.IsSuccessStatusCode)
                        throw new Exception("Upload failed");

                    _bytesUploaded += bytesRead;
                    SaveUploadedBytes(filePath, _bytesUploaded);
                    _progress = (double)_bytesUploaded / _fileSize;
                }

                _stopwatch.Stop();
            }
        }

        public void PauseUpload()
        {
            _isPaused = true;
        }

        public void ResumeUpload()
        {
            _isPaused = false;
        }

        public void CancelUpload()
        {
            _cancellationTokenSource.Cancel();
        }

        public double GetProgress()
        {
            return _progress;
        }

        public TimeSpan GetElapsedTime()
        {
            return _stopwatch.Elapsed;
        }

        private void SaveUploadedBytes(string filePath, long bytesUploaded)
        {
            var key = GetPreferencesKey(filePath);
            Preferences.Set(key, bytesUploaded);
        }

        private long LoadUploadedBytes(string filePath)
        {
            var key = GetPreferencesKey(filePath);
            return Preferences.Get(key, 0L);
        }

        private string GetPreferencesKey(string filePath)
        {
            return $"upload_{Path.GetFileName(filePath)}_bytesUploaded";
        }
    }
}