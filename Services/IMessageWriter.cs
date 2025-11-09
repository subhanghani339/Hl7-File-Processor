namespace Hl7FileProcessor.Services;

public interface IMessageWriter
{
    Task WriteAsync(Hl7Message message, CancellationToken cancellationToken);
}

