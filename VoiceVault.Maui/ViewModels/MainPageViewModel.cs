using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using VoiceVault.Maui.Services;

namespace VoiceVault.Maui.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly IUploadService _uploadService;
        private double _progress;

        public MainPageViewModel(IUploadService uploadService)
        {
            _uploadService = uploadService;
           // _uploadService.ProgressChanged += (s, e) => Progress = _uploadService.Progress;
            UploadCommand = new Command(async () => await UploadFileAsync());
        }

        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }

        public ICommand UploadCommand { get; }

        private async Task UploadFileAsync()
        {
            var filePath = "path_to_your_file"; // Replace with actual file path
            await _uploadService.UploadFileAsync(filePath);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}