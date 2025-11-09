using Hl7FileProcessor.Configuration;
using Hl7FileProcessor.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hl7FileProcessor;

public sealed class Worker : BackgroundService
{
    private readonly IExcelReader _excelReader;
    private readonly IHl7MessageFactory _hl7MessageFactory;
    private readonly ILogger<Worker> _logger;
    private readonly IMessageWriter _messageWriter;
    private readonly ProcessingOptions _options;
    private readonly TimeSpan _pollingInterval;

    public Worker(
        IExcelReader excelReader,
        IHl7MessageFactory hl7MessageFactory,
        IMessageWriter messageWriter,
        IOptions<ProcessingOptions> options,
        ILogger<Worker> logger)
    {
        _excelReader = excelReader;
        _hl7MessageFactory = hl7MessageFactory;
        _messageWriter = messageWriter;
        _logger = logger;
        _options = options.Value;
        if (_options.PollingIntervalMinutes <= 0)
        {
            throw new InvalidOperationException("Processing:PollingIntervalMinutes must be greater than zero.");
        }

        _pollingInterval = TimeSpan.FromMinutes(_options.PollingIntervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HL7 File Processor service started. Polling {InputFolder} every {Interval} minutes.", _options.InputFolder, _pollingInterval.TotalMinutes);

        var timer = new PeriodicTimer(_pollingInterval);
        try
        {
            do
            {
                try
                {
                    await ProcessInputFilesAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Processing cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while processing files.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
        }
        finally
        {
            timer.Dispose();
        }
    }

    private async Task ProcessInputFilesAsync(CancellationToken cancellationToken)
    {
        var pendingFiles = Directory
            .EnumerateFiles(_options.InputFolder, "*.xlsx", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (pendingFiles.Count == 0)
        {
            _logger.LogInformation("No Excel files found in {InputFolder}.", _options.InputFolder);
            return;
        }

        foreach (var filePath in pendingFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("Processing file {FilePath}.", filePath);
            await ProcessFileAsync(filePath, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ProcessFileAsync(string filePath, CancellationToken cancellationToken)
    {
        using var stream = File.OpenRead(filePath);
        var rows = _excelReader.ReadRows(stream);

        var messageTasks = rows.Select(
            row =>
            {
                var message = _hl7MessageFactory.CreateAdmissionMessage(row);
                return _messageWriter.WriteAsync(message, cancellationToken);
            });

        await Task.WhenAll(messageTasks).ConfigureAwait(false);

        var processedDirectory = Path.Combine(_options.InputFolder, "processed");
        Directory.CreateDirectory(processedDirectory);

        var fileName = Path.GetFileName(filePath);
        var destinationPath = Path.Combine(processedDirectory, $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}_{fileName}");
        File.Move(filePath, destinationPath, overwrite: false);

        _logger.LogInformation("Finished processing file {FilePath}. Archived to {DestinationPath}.", filePath, destinationPath);
    }
}

