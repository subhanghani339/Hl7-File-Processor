namespace Hl7FileProcessor.Services;

public interface IFileBootstrapper
{
    Task PrepareAsync(CancellationToken cancellationToken);
}

