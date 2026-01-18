using Microsoft.AspNetCore.Mvc;
using AudioUploadApi.Data;
using AudioUploadApi.Services;
using AudioUploadApi.Entities;

namespace AudioUploadApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AudioController : ControllerBase
{
    private readonly IAudioStorageService _storageService;
    private readonly IAudioCompressionService _compressionService;
    private readonly AppDbContext _context;
    private readonly ILogger<AudioController> _logger;

    public AudioController(
        IAudioStorageService storageService,
        IAudioCompressionService compressionService,
        AppDbContext context,
        ILogger<AudioController> logger)
    {
        _storageService = storageService;
        _compressionService = compressionService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadAudio(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Arquivo não fornecido");

        _logger.LogInformation("Recebido arquivo para upload: {FileName}, Tamanho: {Size} bytes", 
            file.FileName, file.Length);

        // Comprimir áudio para AAC
        var compressedStream = await _compressionService.CompressToAacAsync(file.OpenReadStream(), file.FileName);

        // Gerar novo nome de arquivo com extensão .aac
        var compressedFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}.aac";

        // Upload do arquivo comprimido
        var fileUrl = await _storageService.UploadAudioAsync(compressedStream, compressedFileName);

        var audioFile = new AudioFile
        {
            Id = Guid.NewGuid(),
            FileName = compressedFileName,
            FileUrl = fileUrl,
            UploadedAt = DateTime.UtcNow
        };

        _context.AudioFiles.Add(audioFile);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Upload concluído: {FileName}, URL: {FileUrl}", compressedFileName, fileUrl);

        return Ok(audioFile);
    }

    [HttpGet]
    public async Task<IActionResult> GetAudioFiles()
    {
        var files = await _context.AudioFiles.ToListAsync();
        return Ok(files);
    }
}