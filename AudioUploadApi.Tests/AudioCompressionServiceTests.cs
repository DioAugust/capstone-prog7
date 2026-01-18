using AudioUploadApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AudioUploadApi.Tests;

public class AudioCompressionServiceTests
{
    private readonly FFmpegAudioCompressionService _service;
    private readonly Mock<ILogger<FFmpegAudioCompressionService>> _loggerMock;

    public AudioCompressionServiceTests()
    {
        _loggerMock = new Mock<ILogger<FFmpegAudioCompressionService>>();
        _service = new FFmpegAudioCompressionService(_loggerMock.Object);
    }

    [Fact]
    public async Task CompressToAacAsync_ShouldReturnCompressedStream()
    {
        // Arrange
        var testAudioPath = Path.Combine(Path.GetTempPath(), "test_audio.wav");
        CreateTestWavFile(testAudioPath);

        // Act
        using var inputStream = File.OpenRead(testAudioPath);
        var result = await _service.CompressToAacAsync(inputStream, "test_audio.wav");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        Assert.True(result.CanRead);

        // Cleanup
        File.Delete(testAudioPath);
        result.Dispose();
    }

    [Fact]
    public async Task CompressToAacAsync_ShouldLogCompressionTime()
    {
        // Arrange
        var testAudioPath = Path.Combine(Path.GetTempPath(), "test_audio2.wav");
        CreateTestWavFile(testAudioPath);

        // Act
        using var inputStream = File.OpenRead(testAudioPath);
        await _service.CompressToAacAsync(inputStream, "test_audio2.wav");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Iniciando compressão")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Compressão concluída")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Cleanup
        File.Delete(testAudioPath);
    }

    [Fact]
    public async Task CompressToAacAsync_OutputShouldBeSmallerThanInput()
    {
        // Arrange
        var testAudioPath = Path.Combine(Path.GetTempPath(), "test_audio3.wav");
        CreateTestWavFile(testAudioPath);
        var originalSize = new FileInfo(testAudioPath).Length;

        // Act
        using var inputStream = File.OpenRead(testAudioPath);
        var compressedStream = await _service.CompressToAacAsync(inputStream, "test_audio3.wav");

        // Assert
        Assert.True(compressedStream.Length < originalSize, 
            $"Compressed size ({compressedStream.Length}) should be smaller than original ({originalSize})");

        // Cleanup
        File.Delete(testAudioPath);
        compressedStream.Dispose();
    }

    private void CreateTestWavFile(string path)
    {
        // Create a simple WAV file (1 second, 44100 Hz, mono, 16-bit)
        using var fs = new FileStream(path, FileMode.Create);
        using var writer = new BinaryWriter(fs);

        int sampleRate = 44100;
        short numChannels = 1;
        short bitsPerSample = 16;
        int numSamples = sampleRate; // 1 second

        // WAV header
        writer.Write(new[] { 'R', 'I', 'F', 'F' });
        writer.Write(36 + numSamples * numChannels * bitsPerSample / 8);
        writer.Write(new[] { 'W', 'A', 'V', 'E' });
        writer.Write(new[] { 'f', 'm', 't', ' ' });
        writer.Write(16); // Subchunk1Size
        writer.Write((short)1); // AudioFormat (PCM)
        writer.Write(numChannels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * numChannels * bitsPerSample / 8); // ByteRate
        writer.Write((short)(numChannels * bitsPerSample / 8)); // BlockAlign
        writer.Write(bitsPerSample);
        writer.Write(new[] { 'd', 'a', 't', 'a' });
        writer.Write(numSamples * numChannels * bitsPerSample / 8);

        // Write audio data (simple sine wave)
        for (int i = 0; i < numSamples; i++)
        {
            short sample = (short)(Math.Sin(2 * Math.PI * 440 * i / sampleRate) * 32767 * 0.5);
            writer.Write(sample);
        }
    }
}