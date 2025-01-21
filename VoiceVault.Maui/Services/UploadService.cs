using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
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
        private readonly int _chunkSize = 320 * 1024; // 320 KiB (327,680 bytes)
        private double _progress;

        // New fields
        private long _fileSize;
        private Stopwatch _stopwatch;
        private long _bytesUploaded;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isPaused;

        // Expose current chunk as a property
        public int CurrentChunk { get; private set; }

        public UploadService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Determine the API base URL based on the platform
        #if ANDROID
            var baseAddress = "http://10.0.2.2:5171/";
        #else
            var baseAddress = "http://10.0.0.188:5171/";
        #endif

            _httpClient.BaseAddress = new Uri(baseAddress);
            _apiUrl = "api/upload";
            _completeUrl = "api/complete";
            _cancellationTokenSource = new CancellationTokenSource();
            _stopwatch = new Stopwatch();
            ProgressChanged = delegate { };
        }
        {
            _httpClient = httpClient;

            // Determine the API base URL based on the platform
        #if ANDROID
            var baseAddress = "http://10.0.2.2:5171/";
        #else
            var baseAddress = "http://10.0.0.188:5171/";
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

            // 1. Create upload session
            var fileSize = fileStream.Length;
            var createSessionUrl = $"{_httpClient.BaseAddress}api/createUploadSession?fileName={fileName}&fileSize={fileSize}";
            var sessionResponse = await _httpClient.PostAsync(createSessionUrl, null, _cancellationTokenSource.Token);
            sessionResponse.EnsureSuccessStatusCode();

            var sessionJson = await sessionResponse.Content.ReadAsStringAsync(_cancellationTokenSource.Token);
            var sessionData = JsonSerializer.Deserialize<CreateSessionResponse>(sessionJson);

            // sessionData.uploadUrl = PUT chunk endpoint
            // sessionData.sessionId
            // sessionData.expirationDateTime

            _fileSize = fileSize;
            _stopwatch = Stopwatch.StartNew();
            _bytesUploaded = LoadUploadedBytes(fileName);
            _isPaused = false;

            // Move stream position to where we left off
            fileStream.Seek(_bytesUploaded, SeekOrigin.Begin);

            var chunkData = new byte[_chunkSize];
            var rangeStart = _bytesUploaded;
            int bytesRead;
            while ((bytesRead = await fileStream.ReadAsync(chunkData, 0, chunkData.Length, _cancellationTokenSource.Token)) > 0)
            {
                if (_isPaused)
                {
                    // Query the API to get the next expected range
                    var nextRangeUrl = $"{_httpClient.BaseAddress}api/uploadChunk/{sessionData.sessionId}/nextRange";
                    var nextRangeResponse = await _httpClient.GetAsync(nextRangeUrl, _cancellationTokenSource.Token);
                    nextRangeResponse.EnsureSuccessStatusCode();

                    var nextRangeJson = await nextRangeResponse.Content.ReadAsStringAsync(_cancellationTokenSource.Token);
                    var nextRangeData = JsonSerializer.Deserialize<NextRangeResponse>(nextRangeJson);

                    // Update the rangeStart based on the next expected range
                    rangeStart = nextRangeData.NextExpectedRangeStart;
                    fileStream.Seek(rangeStart, SeekOrigin.Begin);
                    _bytesUploaded = rangeStart;
                    _isPaused = false;
                }

                var rangeEnd = rangeStart + bytesRead - 1;

                // 2. Upload a chunk with PUT
                var chunkEndpoint = $"{_httpClient.BaseAddress}api/uploadChunk/{sessionData.sessionId}?rangeStart={rangeStart}&rangeEnd={rangeEnd}";
                using var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(chunkData, 0, bytesRead), "chunk", fileName);

                var response = await _httpClient.PutAsync(chunkEndpoint, content, _cancellationTokenSource.Token);
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Chunk upload failed: {response.StatusCode}");

                _bytesUploaded += bytesRead;
                SaveUploadedBytes(fileName, _bytesUploaded);
                Progress = (double)_bytesUploaded / _fileSize;

                rangeStart += bytesRead;
            }

            _stopwatch.Stop();

            // 3. Complete upload session
            var completeUrl = $"{_httpClient.BaseAddress}api/completeUpload/{sessionData.sessionId}";
            var completeResponse = await _httpClient.PostAsync(completeUrl, null, _cancellationTokenSource.Token);
            completeResponse.EnsureSuccessStatusCode();
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

        // Minimal DTO to parse the session JSON response
        private record CreateSessionResponse(string uploadUrl, DateTime expirationDateTime, string sessionId);

        // DTO to parse the next expected range JSON response
        private record NextRangeResponse(long NextExpectedRangeStart);
    }
}