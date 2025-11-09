using Hl7FileProcessor.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hl7FileProcessor.Services;

public sealed class FileMessageWriter : IMessageWriter
{
    private readonly ILogger<FileMessageWriter> _logger;
    private readonly ProcessingOptions _options;

    public FileMessageWriter(IOptions<ProcessingOptions> options, ILogger<FileMessageWriter> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task WriteAsync(Hl7Message message, CancellationToken cancellationToken)
    {
        var outputPath = Path.Combine(_options.OutputFolder, message.FileName);
        Directory.CreateDirectory(_options.OutputFolder);

        await File.WriteAllTextAsync(outputPath, message.Content, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Wrote HL7 message to {OutputPath}.", outputPath);
    }
}

