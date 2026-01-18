namespace AudioUploadApi.Services;

public interface IAudioCompressionService
{
    Task<Stream> CompressToAacAsync(Stream inputStream, string originalFileName);
}