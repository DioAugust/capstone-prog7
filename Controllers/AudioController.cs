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
    private readonly AppDbContext _context;

    public AudioController(IAudioStorageService storageService, AppDbContext context)
    {
        _storageService = storageService;
        _context = context;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadAudio(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Arquivo não fornecido");

        var fileUrl = await _storageService.UploadAudioAsync(file.OpenReadStream(), file.FileName);

        var audioFile = new AudioFile
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            FileUrl = fileUrl,
            UploadedAt = DateTime.UtcNow
        };

        _context.AudioFiles.Add(audioFile);
        await _context.SaveChangesAsync();

        return Ok(audioFile);
    }

    [HttpGet]
    public async Task<IActionResult> GetAudioFiles()
    {
        var files = await _context.AudioFiles.ToListAsync();
        return Ok(files);
    }
}