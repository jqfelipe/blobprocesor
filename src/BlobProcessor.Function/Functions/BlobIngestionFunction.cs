using BlobProcessor.Function.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BlobProcessor.Function.Functions;

public sealed class BlobIngestionFunction
{
    private readonly ILogger<BlobIngestionFunction> _logger;
    private readonly IFileProcessor _fileProcessor;
    private readonly IMessagePublisher _publisher;

    public BlobIngestionFunction(
        ILogger<BlobIngestionFunction> logger,
        IFileProcessor fileProcessor,
        IMessagePublisher publisher)
    {
        _logger = logger;
        _fileProcessor = fileProcessor;
        _publisher = publisher;
    }

    [Function("BlobCreatedProcessor")]
    public async Task RunAsync(
        [BlobTrigger("%BlobContainerName%/{name}", Connection = "BlobStorageConnection")]
        Stream blobStream,
        string name,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Blob trigger fired for file: {BlobName}", name);

        using var reader = new StreamReader(blobStream);
        var content = await reader.ReadToEndAsync();

        var processedMessage = _fileProcessor.Process(content, name);
        await _publisher.PublishAsync(processedMessage, cancellationToken);

        _logger.LogInformation("Blob {BlobName} processed and sent to topic", name);
    }
}
