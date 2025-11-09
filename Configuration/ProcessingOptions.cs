namespace Hl7FileProcessor.Configuration;

public sealed class ProcessingOptions
{
    public const string SectionName = "Processing";

    public required string InputFolder { get; init; }

    public required string OutputFolder { get; init; }

    public required int PollingIntervalMinutes { get; init; }
}

