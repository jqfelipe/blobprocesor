namespace BlobProcessor.Function.Models;

public sealed class ProcessedBlobMessage
{
    public required string BlobName { get; init; }
    public required DateTime ProcessedAtUtc { get; init; }
    public required int ContentLength { get; init; }
    public required int LineCount { get; init; }
    public required string Sha256 { get; init; }
    public required string ProcessedContent { get; init; }
}
