using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly IAudioSummaryService _summaryService;
    private readonly AppDbContext _context;
    private readonly ILogger<AudioController> _logger;

    public AudioController(
        IAudioStorageService storageService,
        IAudioCompressionService compressionService,
        IAudioSummaryService summaryService,
        AppDbContext context,
        ILogger<AudioController> logger)
    {
        _storageService = storageService;
        _compressionService = compressionService;
        _summaryService = summaryService;
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

        // Criar cópia do stream para processamento paralelo
        var originalStream = new MemoryStream();
        await file.OpenReadStream().CopyToAsync(originalStream);
        originalStream.Position = 0;

        var compressionStream = new MemoryStream();
        await originalStream.CopyToAsync(compressionStream);
        compressionStream.Position = 0;
        originalStream.Position = 0;

        // Processar compressão e extração de resumo em paralelo usando threading
        var compressionTask = _compressionService.CompressToAacAsync(compressionStream, file.FileName);
        var summaryTask = _summaryService.ExtractSummaryAsync(originalStream, file.FileName);

        // Aguardar ambas as operações completarem
        await Task.WhenAll(compressionTask, summaryTask);

        var compressedStream = await compressionTask;
        var summary = await summaryTask;

        // Gerar novo nome de arquivo com extensão .aac
        var compressedFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}.aac";

        // Upload do arquivo comprimido
        var fileUrl = await _storageService.UploadAudioAsync(compressedStream, compressedFileName);

        var audioFile = new AudioFile
        {
            Id = Guid.NewGuid(),
            FileName = compressedFileName,
            FileUrl = fileUrl,
            Summary = summary,
            UploadedAt = DateTime.UtcNow
        };

        _context.AudioFiles.Add(audioFile);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Upload concluído: {FileName}, URL: {FileUrl}, Resumo: {Summary}", 
            compressedFileName, fileUrl, summary);

        return Ok(audioFile);
    }

    [HttpGet]
    public async Task<IActionResult> GetAudioFiles()
    {
        var files = await _context.AudioFiles.ToListAsync();
        return Ok(files);
    }
}