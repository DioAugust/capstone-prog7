using Azure.Storage.Blobs;

namespace AudioUploadApi.Services;

public class AzureBlobStorageService : IAudioStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadAudioAsync(Stream audioStream, string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient("audio-files");
        await containerClient.CreateIfNotExistsAsync();
        
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(audioStream, overwrite: true);
        
        return blobClient.Uri.ToString();
    }
}