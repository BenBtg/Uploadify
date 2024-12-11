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

        public MainViewModel(IUploadService uploadService)
        {
            _uploadService = uploadService;
            _uploadService.ProgressChanged += (s, e) => Progress = _uploadService.Progress;
            UploadCommand = new Command(async () => await UploadFileAsync());
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
                }
            }
        }

        public ICommand UploadCommand { get; }

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
                // Handle exceptions (e.g., user cancels picking a file)
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}