namespace AudioUploadApi.Entities;

public class AudioFile
{
    public Guid Id { get; set; }
    public string FileUrl { get; set; }
    public string FileName { get; set; }
    public DateTime UploadedAt { get; set; }
}