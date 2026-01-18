using System.Diagnostics;

namespace AudioUploadApi.Services;

public class FFmpegAudioCompressionService : IAudioCompressionService
{
    private readonly ILogger<FFmpegAudioCompressionService> _logger;

    public FFmpegAudioCompressionService(ILogger<FFmpegAudioCompressionService> logger)
    {
        _logger = logger;
    }

    public async Task<Stream> CompressToAacAsync(Stream inputStream, string originalFileName)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Iniciando compressão de áudio: {FileName}", originalFileName);

        var tempInputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}");
        var tempOutputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.aac");

        try
        {
            // Salvar stream de entrada em arquivo temporário
            await using (var fileStream = File.Create(tempInputPath))
            {
                await inputStream.CopyToAsync(fileStream);
            }

            // Executar FFmpeg para comprimir para AAC
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{tempInputPath}\" -c:a aac -b:a 128k -y \"{tempOutputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
                throw new Exception("Falha ao iniciar processo FFmpeg");

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                throw new Exception($"FFmpeg falhou: {error}");
            }

            stopwatch.Stop();
            _logger.LogInformation("Compressão concluída em {ElapsedMs}ms para {FileName}", 
                stopwatch.ElapsedMilliseconds, originalFileName);

            // Ler arquivo comprimido e retornar como stream
            var compressedStream = new MemoryStream();
            await using (var fileStream = File.OpenRead(tempOutputPath))
            {
                await fileStream.CopyToAsync(compressedStream);
            }
            compressedStream.Position = 0;

            return compressedStream;
        }
        finally
        {
            // Limpar arquivos temporários
            if (File.Exists(tempInputPath))
                File.Delete(tempInputPath);
            if (File.Exists(tempOutputPath))
                File.Delete(tempOutputPath);
        }
    }
}