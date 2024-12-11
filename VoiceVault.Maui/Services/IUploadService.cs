namespace VoiceVault.Maui.Services
{
    public interface IUploadService
    {
        Task UploadFileAsync(string filePath);
        double Progress { get; }
        event EventHandler ProgressChanged;
    }
}