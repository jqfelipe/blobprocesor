using BlobProcessor.Function.Services;
using Xunit;

namespace BlobProcessor.Function.Tests.Services;

public sealed class FileProcessorTests
{
    private readonly FileProcessor _sut = new();

    [Fact]
    public void Process_ValidCsvContent_ReturnsProcessedMessage()
    {
        var csv = "ES1234567890123456789012,Juan Perez,1\nES9876543210987654321098,Ana Lopez,2";

        var result = _sut.Process(csv, "test.csv");

        Assert.Equal("test.csv", result.BlobName);
        Assert.Equal(2, result.LineCount);
        Assert.NotEmpty(result.ProcessedContent);
        Assert.NotEmpty(result.Sha256);
        Assert.True(result.ContentLength > 0);
    }

    [Fact]
    public void Process_ContentWithInvalidLines_SkipsInvalidLines()
    {
        var csv = "ES1234567890123456789012,Juan Perez,1\nINVALID_LINE\nES9876543210987654321098,Ana Lopez,2";

        var result = _sut.Process(csv, "test.csv");

        Assert.Contains("ES1234567890123456789012", result.ProcessedContent);
        Assert.Contains("ES9876543210987654321098", result.ProcessedContent);
    }

    [Fact]
    public void Process_ContentWithOnlyInvalidLines_ReturnsEmptyJsonArray()
    {
        var csv = "NOT_VALID\nALSO_NOT_VALID";

        var result = _sut.Process(csv, "test.csv");

        Assert.Equal("[]", result.ProcessedContent);
    }

    [Fact]
    public void Process_NullFileContent_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.Process(null!, "test.csv"));
    }

    [Fact]
    public void Process_WhitespaceFileContent_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.Process("   ", "test.csv"));
    }

    [Fact]
    public void Process_NullBlobName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.Process("ES1234,Titular,1", null!));
    }

    [Fact]
    public void Process_WhitespaceBlobName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.Process("ES1234,Titular,1", "   "));
    }

    [Fact]
    public void Process_ValidContent_SetsProcessedAtUtcToNow()
    {
        var before = DateTime.UtcNow;
        var result = _sut.Process("ES1234,Titular,1", "test.csv");
        var after = DateTime.UtcNow;

        Assert.InRange(result.ProcessedAtUtc, before, after);
    }

    [Fact]
    public void Process_SameContent_ProducesSameSha256()
    {
        var csv = "ES1234567890123456789012,Juan Perez,1";

        var result1 = _sut.Process(csv, "blob1.csv");
        var result2 = _sut.Process(csv, "blob2.csv");

        Assert.Equal(result1.Sha256, result2.Sha256);
    }
}
