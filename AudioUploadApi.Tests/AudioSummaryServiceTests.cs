using AudioUploadApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AudioUploadApi.Tests;

public class AudioSummaryServiceTests
{
    private readonly AudioSummaryService _service;
    private readonly Mock<ILogger<AudioSummaryService>> _loggerMock;

    public AudioSummaryServiceTests()
    {
        _loggerMock = new Mock<ILogger<AudioSummaryService>>();
        _service = new AudioSummaryService(_loggerMock.Object);
    }

    [Fact]
    public async Task ExtractSummaryAsync_ShouldReturnSummaryWithMaximum50Characters()
    {
        // Arrange
        var stream = new MemoryStream(new byte[1024]);
        var fileName = "test_audio_file.mp3";

        // Act
        var summary = await _service.ExtractSummaryAsync(stream, fileName);

        // Assert
        Assert.NotNull(summary);
        Assert.True(summary.Length <= 50, $"Summary length ({summary.Length}) exceeds 50 characters");
    }

    [Fact]
    public async Task ExtractSummaryAsync_WithLongFileName_ShouldTruncateTo50Characters()
    {
        // Arrange
        var stream = new MemoryStream(new byte[5000]);
        var longFileName = "this_is_a_very_long_audio_file_name_that_should_be_truncated_to_fit_the_limit.wav";

        // Act
        var summary = await _service.ExtractSummaryAsync(stream, longFileName);

        // Assert
        Assert.NotNull(summary);
        Assert.True(summary.Length <= 50);
        Assert.NotEmpty(summary);
    }

    [Fact]
    public async Task ExtractSummaryAsync_ShouldLogExtractionTime()
    {
        // Arrange
        var stream = new MemoryStream(new byte[2048]);
        var fileName = "audio.mp3";

        // Act
        await _service.ExtractSummaryAsync(stream, fileName);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Iniciando extração")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Extração de resumo concluída")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExtractSummaryAsync_ShouldReturnNonEmptySummary()
    {
        // Arrange
        var stream = new MemoryStream(new byte[512]);
        var fileName = "short.aac";

        // Act
        var summary = await _service.ExtractSummaryAsync(stream, fileName);

        // Assert
        Assert.NotNull(summary);
        Assert.NotEmpty(summary);
    }

    [Theory]
    [InlineData("audio.mp3", 100)]
    [InlineData("podcast.wav", 5000)]
    [InlineData("music.aac", 50000)]
    public async Task ExtractSummaryAsync_WithDifferentFileSizes_ShouldAlwaysRespectLimit(string fileName, int fileSize)
    {
        // Arrange
        var stream = new MemoryStream(new byte[fileSize]);

        // Act
        var summary = await _service.ExtractSummaryAsync(stream, fileName);

        // Assert
        Assert.True(summary.Length <= 50, 
            $"Summary for {fileName} with size {fileSize} exceeded limit: {summary.Length} characters");
    }
}