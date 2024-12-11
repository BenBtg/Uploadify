namespace VoiceVault.Maui.Services
{
    public interface IUploadService
    {
        Task UploadFileAsync(Stream fileStream, string fileName);
        double Progress { get; }
        event EventHandler ProgressChanged;

        // New properties
        long FileSize { get; }
        DateTime StartTime { get; }
        TimeSpan ElapsedTime { get; }
        TimeSpan EstimatedTimeToCompletion { get; }
    }
}