namespace AudioUploadApi.Services;

public interface IAudioSummaryService
{
    Task<string> ExtractSummaryAsync(Stream audioStream, string fileName);
}