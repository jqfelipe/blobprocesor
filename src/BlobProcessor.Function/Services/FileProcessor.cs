using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BlobProcessor.Function.Models;

namespace BlobProcessor.Function.Services;

public sealed class FileProcessor : IFileProcessor
{
  public ProcessedBlobMessage Process(string fileContent, string blobName)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(fileContent, nameof(fileContent));
    ArgumentException.ThrowIfNullOrWhiteSpace(blobName, nameof(blobName));

    var normalized = fileContent.Trim();
    var lineCount = CountLines(normalized);
    
    // Parse CSV lines to JSON
    var records = ParseCsvToRecords(normalized);
    var jsonContent = JsonSerializer.Serialize(records);
    var hash = ComputeSha256(jsonContent);

    return new ProcessedBlobMessage
    {
      BlobName = blobName,
      ProcessedAtUtc = DateTime.UtcNow,
      ContentLength = jsonContent.Length,
      LineCount = lineCount,
      Sha256 = hash,
      ProcessedContent = jsonContent
    };
  }

  private static List<CuentaRecord> ParseCsvToRecords(string content)
  {
    var records = new List<CuentaRecord>();
    
    if (string.IsNullOrWhiteSpace(content))
    {
      return records;
    }

    var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    
    foreach (var line in lines)
    {
      var trimmedLine = line.Trim();
      if (string.IsNullOrWhiteSpace(trimmedLine))
      {
        continue;
      }

      var parts = trimmedLine.Split(',', StringSplitOptions.None);
      
      if (parts.Length != 3)
      {
        continue; // Skip invalid lines
      }

      var cuentaIBAN = parts[0].Trim();
      var titular = parts[1].Trim();
      
      if (short.TryParse(parts[2].Trim(), out var accion))
      {
        records.Add(new CuentaRecord
        {
          CuentaIBAN = cuentaIBAN,
          Titular = titular,
          Accion = accion
        });
      }
    }

    return records;
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

public sealed class CuentaRecord
{
  public string CuentaIBAN { get; set; } = string.Empty;
  public string Titular { get; set; } = string.Empty;
  public short Accion { get; set; }
}
