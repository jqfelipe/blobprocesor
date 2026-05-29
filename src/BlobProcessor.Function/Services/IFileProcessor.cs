using BlobProcessor.Function.Models;

namespace BlobProcessor.Function.Services;

public interface IFileProcessor
{
    ProcessedBlobMessage Process(string fileContent, string blobName);
}
