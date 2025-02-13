namespace Uploadify.Maui.Services
{
    public interface IUploadService
    {
        Task UploadFileAsync(Stream fileStream, string fileName);
        void ResumeUpload();
        void PauseUpload();

        double Progress { get; }
        event EventHandler ProgressChanged;

        long FileSize { get; }
        DateTime StartTime { get; }
        TimeSpan ElapsedTime { get; }
        TimeSpan EstimatedTimeToCompletion { get; }

        // Expose current chunk
        int CurrentChunk { get; }
    }
}