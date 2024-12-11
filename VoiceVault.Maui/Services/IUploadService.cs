namespace VoiceVault.Maui.Services
{
    public interface IUploadService
    {
        Task UploadFileAsync(Stream fileStream, string fileName);
        double Progress { get; }
        event EventHandler ProgressChanged;
    }
}