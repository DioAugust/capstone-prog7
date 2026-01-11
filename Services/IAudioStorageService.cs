namespace AudioUploadApi.Services;

public interface IAudioStorageService
{
    Task<string> UploadAudioAsync(Stream audioStream, string fileName);
}