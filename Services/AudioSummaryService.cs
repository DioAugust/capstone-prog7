using System.Diagnostics;
using System.Text;

namespace AudioUploadApi.Services;

public class AudioSummaryService : IAudioSummaryService
{
    private readonly ILogger<AudioSummaryService> _logger;
    private const int MaxSummaryLength = 50;

    public AudioSummaryService(ILogger<AudioSummaryService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ExtractSummaryAsync(Stream audioStream, string fileName)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Iniciando extração de resumo: {FileName}", fileName);

        try
        {
            // Usar threading para processar análise de áudio em paralelo
            var tasks = new List<Task<string>>
            {
                Task.Run(() => ExtractMetadataInfo(fileName)),
                Task.Run(() => AnalyzeAudioCharacteristics(audioStream)),
                Task.Run(() => GenerateTimestamp())
            };

            // Aguardar todas as tasks completarem em paralelo
            var results = await Task.WhenAll(tasks);

            // Combinar resultados
            var summary = CombineResults(results);

            stopwatch.Stop();
            _logger.LogInformation("Extração de resumo concluída em {ElapsedMs}ms para {FileName}", 
                stopwatch.ElapsedMilliseconds, fileName);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao extrair resumo de {FileName}", fileName);
            return GenerateFallbackSummary(fileName);
        }
    }

    private string ExtractMetadataInfo(string fileName)
    {
        // Simula extração de metadados (modelo leve)
        Thread.Sleep(100); // Simula processamento
        var extension = Path.GetExtension(fileName).TrimStart('.');
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        
        return $"{extension.ToUpper()}:{nameWithoutExt}";
    }

    private string AnalyzeAudioCharacteristics(Stream audioStream)
    {
        // Simula análise de características do áudio (modelo leve)
        Thread.Sleep(150); // Simula processamento
        
        var sizeKb = audioStream.Length / 1024;
        string sizeCategory = sizeKb switch
        {
            < 100 => "Curto",
            < 1000 => "Médio",
            _ => "Longo"
        };

        return sizeCategory;
    }

    private string GenerateTimestamp()
    {
        // Simula geração de timestamp (processamento rápido)
        Thread.Sleep(50); // Simula processamento
        return DateTime.UtcNow.ToString("HHmm");
    }

    private string CombineResults(string[] results)
    {
        var combined = string.Join(" ", results.Where(r => !string.IsNullOrEmpty(r)));
        
        // Truncar para 50 caracteres
        if (combined.Length > MaxSummaryLength)
        {
            combined = combined.Substring(0, MaxSummaryLength - 3) + "...";
        }

        return combined;
    }

    private string GenerateFallbackSummary(string fileName)
    {
        var fallback = $"Audio: {Path.GetFileNameWithoutExtension(fileName)}";
        return fallback.Length > MaxSummaryLength 
            ? fallback.Substring(0, MaxSummaryLength - 3) + "..."
            : fallback;
    }
}