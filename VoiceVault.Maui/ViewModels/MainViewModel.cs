using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using VoiceVault.Maui.Services;

namespace VoiceVault.Maui.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IUploadService _uploadService;
        private double _progress;
        private bool _isPaused;
        private string _pauseResumeButtonText;

        public MainViewModel(IUploadService uploadService)
        {
            _uploadService = uploadService;
            _uploadService.ProgressChanged += OnProgressChanged;
            UploadCommand = new Command(async () => await UploadFileAsync());
            PauseResumeCommand = new Command(PauseResumeUpload);
            _pauseResumeButtonText = "Pause";
        }

        public double Progress
        {
            get => _progress;
            set
            {
                if (Math.Abs(_progress - value) > 0.01)
                {
                    _progress = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ProgressDisplay));
                    OnPropertyChanged(nameof(ElapsedTimeDisplay));
                    OnPropertyChanged(nameof(EstimatedTimeDisplay));
                }
            }
        }

        public string ProgressDisplay => $"{Progress * 100:F0}% Completed";
        public string FileSizeDisplay => $"{(_uploadService.FileSize / (1024.0 * 1024.0)):F2} MB";
        public string StartTimeDisplay => _uploadService.StartTime.ToString("T");
        public string ElapsedTimeDisplay => _uploadService.ElapsedTime.ToString(@"mm\:ss");
        public string EstimatedTimeDisplay => _uploadService.EstimatedTimeToCompletion.TotalSeconds > 0
            ? $"{_uploadService.EstimatedTimeToCompletion:mm\\:ss} remaining"
            : "Calculating...";

        public string PauseResumeButtonText
        {
            get => _pauseResumeButtonText;
            set
            {
                _pauseResumeButtonText = value;
                OnPropertyChanged();
            }
        }

        public ICommand UploadCommand { get; }
        public ICommand PauseResumeCommand { get; }

        private async Task UploadFileAsync()
        {
            try
            {
                var fileResult = await FilePicker.PickAsync();
                if (fileResult != null)
                {
                    using var stream = await fileResult.OpenReadAsync();
                    await _uploadService.UploadFileAsync(stream, fileResult.FileName);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
        }

        private void PauseResumeUpload()
        {
            if (_isPaused)
            {
                _uploadService.ResumeUpload();
                PauseResumeButtonText = "Pause";
            }
            else
            {
                _uploadService.PauseUpload();
                PauseResumeButtonText = "Resume";
            }

            _isPaused = !_isPaused;
        }

        private void OnProgressChanged(object sender, EventArgs e)
        {
            Progress = _uploadService.Progress;
            OnPropertyChanged(nameof(FileSizeDisplay));
            OnPropertyChanged(nameof(StartTimeDisplay));
            OnPropertyChanged(nameof(ElapsedTimeDisplay));
            OnPropertyChanged(nameof(EstimatedTimeDisplay));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}