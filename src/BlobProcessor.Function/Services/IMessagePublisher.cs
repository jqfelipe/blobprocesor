using BlobProcessor.Function.Models;

namespace BlobProcessor.Function.Services;

public interface IMessagePublisher
{
    Task PublishAsync(ProcessedBlobMessage message, CancellationToken cancellationToken);
}
