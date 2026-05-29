using BlobProcessor.Function.Functions;
using BlobProcessor.Function.Models;
using BlobProcessor.Function.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BlobProcessor.Function.Tests.Functions;

public sealed class BlobIngestionFunctionTests
{
    private readonly Mock<ILogger<BlobIngestionFunction>> _loggerMock = new();
    private readonly Mock<IFileProcessor> _fileProcessorMock = new();
    private readonly Mock<IMessagePublisher> _publisherMock = new();
    private readonly BlobIngestionFunction _sut;

    public BlobIngestionFunctionTests()
    {
        _sut = new BlobIngestionFunction(_loggerMock.Object, _fileProcessorMock.Object, _publisherMock.Object);
    }

    [Fact]
    public async Task RunAsync_ValidBlob_ProcessesAndPublishes()
    {
        var blobName = "test.csv";
        var content = "ES1234,Titular,1";
        var processedMessage = CreateSampleMessage(blobName);

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        _fileProcessorMock.Setup(x => x.Process(content, blobName)).Returns(processedMessage);
        _publisherMock.Setup(x => x.PublishAsync(processedMessage, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.RunAsync(stream, blobName, CancellationToken.None);

        _fileProcessorMock.Verify(x => x.Process(content, blobName), Times.Once);
        _publisherMock.Verify(x => x.PublishAsync(processedMessage, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_FileProcessorThrowsArgumentException_RethrowsAndLogs()
    {
        var blobName = "invalid.csv";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        _fileProcessorMock.Setup(x => x.Process(It.IsAny<string>(), blobName))
            .Throws(new ArgumentException("Invalid content"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.RunAsync(stream, blobName, CancellationToken.None));

        _publisherMock.Verify(x => x.PublishAsync(It.IsAny<ProcessedBlobMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_PublisherThrowsInvalidOperationException_RethrowsAndLogs()
    {
        var blobName = "test.csv";
        var processedMessage = CreateSampleMessage(blobName);

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("ES1234,Titular,1"));
        _fileProcessorMock.Setup(x => x.Process(It.IsAny<string>(), blobName)).Returns(processedMessage);
        _publisherMock.Setup(x => x.PublishAsync(processedMessage, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service Bus not available"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.RunAsync(stream, blobName, CancellationToken.None));
    }

    [Fact]
    public async Task RunAsync_UnexpectedException_RethrowsAndLogs()
    {
        var blobName = "test.csv";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        _fileProcessorMock.Setup(x => x.Process(It.IsAny<string>(), blobName))
            .Throws(new Exception("Unexpected failure"));

        await Assert.ThrowsAsync<Exception>(() =>
            _sut.RunAsync(stream, blobName, CancellationToken.None));
    }

    private static ProcessedBlobMessage CreateSampleMessage(string blobName) =>
        new()
        {
            BlobName = blobName,
            ProcessedAtUtc = DateTime.UtcNow,
            ContentLength = 10,
            LineCount = 1,
            Sha256 = "ABCD1234",
            ProcessedContent = "[]"
        };
}
