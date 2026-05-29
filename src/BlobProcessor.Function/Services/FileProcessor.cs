using System.Security.Cryptography;
using System.Text;
using BlobProcessor.Function.Models;

namespace BlobProcessor.Function.Services;

public sealed class FileProcessor : IFileProcessor
{
    public ProcessedBlobMessage Process(string fileContent, string blobName)
    {
        var normalized = fileContent.Trim();
        var processedContent = normalized.ToUpperInvariant();
        var lineCount = CountLines(normalized);
        var hash = ComputeSha256(processedContent);

        return new ProcessedBlobMessage
        {
            BlobName = blobName,
            ProcessedAtUtc = DateTime.UtcNow,
            ContentLength = processedContent.Length,
            LineCount = lineCount,
            Sha256 = hash,
            ProcessedContent = processedContent
        };
    }

    private static int CountLines(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return 0;
        }

        return value.Split('\n', StringSplitOptions.None).Length;
    }

    private static string ComputeSha256(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
