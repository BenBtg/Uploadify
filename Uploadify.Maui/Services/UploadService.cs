using System.Diagnostics;

namespace Uploadify.Maui.Services
{
    public class UploadService : IUploadService
    {
        private readonly UploadifyApiClient _apiClient;
        private readonly int _chunkSize = 320 * 1024; // 320 KiB (327,680 bytes)
        private double _progress;
        private long _fileSize;
        private Stopwatch _stopwatch;
        private DateTime _startTime;
        private string _sessionId;
        private string _fileName;

        // New fields
        private long _bytesUploaded;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isPaused;

        // Expose current chunk as a property
        public int CurrentChunk { get; private set; }

        public UploadService(UploadifyApiClient apiClient)
        {
            _apiClient = apiClient;
            _cancellationTokenSource = new CancellationTokenSource();
            _stopwatch = new Stopwatch();
        }

        public async Task UploadFileAsync(Stream fileStream, string fileName)
        {
            // 1. Create upload session
            var fileSize = fileStream.Length;
            var sessionData = await _apiClient.CreateUploadSessionAsync(fileName, fileSize);

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
                    var nextRangeData = await _apiClient.GetNextExpectedRangeAsync(sessionData.sessionId);

                    // Update the rangeStart based on the next expected range
                    rangeStart = nextRangeData;
                    fileStream.Seek(rangeStart, SeekOrigin.Begin);
                    _bytesUploaded = rangeStart;
                    _isPaused = false;
                }

                var rangeEnd = rangeStart + bytesRead - 1;

                // 2. Upload a chunk
                await _apiClient.UploadFileChunkAsync(fileStream, sessionData.sessionId, rangeStart, fileName);

                _bytesUploaded += bytesRead;
                SaveUploadedBytes(fileName, _bytesUploaded);
                Progress = (double)_bytesUploaded / _fileSize;

                rangeStart += bytesRead;
            }

            _stopwatch.Stop();

            // 3. Complete upload session
            await _apiClient.CompleteUploadAsync(sessionData.sessionId);
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

        public double Progress
        {
            get => _progress;
            private set
            {
                if (Math.Abs(_progress - value) > 0.01)
                {
                    _progress = value;
                    Preferences.Set("Progress", _progress);
                    ProgressChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler ProgressChanged;

        public long FileSize
        {
            get => _fileSize;
            private set
            {
                _fileSize = value;
                Preferences.Set("FileSize", _fileSize);
            }
        }

        public DateTime StartTime
        {
            get => _startTime;
            private set
            {
                _startTime = value;
                Preferences.Set("StartTime", _startTime.ToString("o"));
            }
        }

        public string SessionId
        {
            get => _sessionId;
            private set
            {
                _sessionId = value;
                Preferences.Set("SessionId", _sessionId);
            }
        }

        public string FileName
        {
            get => _fileName;
            private set
            {
                _fileName = value;
                Preferences.Set("FileName", _fileName);
            }
        }

        public TimeSpan ElapsedTime => _stopwatch?.Elapsed ?? TimeSpan.Zero;

        public TimeSpan EstimatedTimeToCompletion => Progress > 0 
            ? TimeSpan.FromSeconds((1 - Progress) * ElapsedTime.TotalSeconds / Progress) 
            : TimeSpan.MaxValue;

        private void SaveUploadedBytes(string fileName, long bytesUploaded)
        {
            var key = GetPreferencesKey(fileName);
            Preferences.Set(key, bytesUploaded);
        }

        private long LoadUploadedBytes(string fileName)
        {
            var key = GetPreferencesKey(fileName);
            return Preferences.Get(key, 0L);
        }

        private string GetPreferencesKey(string fileName)
        {
            return $"upload_{fileName}_bytesUploaded";
        }
    }
}