using Hl7FileProcessor;
using Hl7FileProcessor.Configuration;
using Hl7FileProcessor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "HL7 File Processor";
    })
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.SetBasePath(AppContext.BaseDirectory);
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddEnvironmentVariables(prefix: "HL7_");
    })
    .ConfigureServices((context, services) =>
    {
        var processingSection = context.Configuration.GetSection(ProcessingOptions.SectionName);
        services.Configure<ProcessingOptions>(processingSection);
        
        var options = processingSection.Get<ProcessingOptions>();
        if (options is null)
        {
            throw new InvalidOperationException($"Configuration section '{ProcessingOptions.SectionName}' is missing or invalid.");
        }
        
        if (string.IsNullOrWhiteSpace(options.InputFolder))
        {
            throw new InvalidOperationException("InputFolder configuration is required.");
        }
        
        if (string.IsNullOrWhiteSpace(options.OutputFolder))
        {
            throw new InvalidOperationException("OutputFolder configuration is required.");
        }
        
        if (options.PollingIntervalMinutes <= 0)
        {
            throw new InvalidOperationException("PollingIntervalMinutes must be greater than 0.");
        }
        
        services.AddSingleton<IFileBootstrapper, FileBootstrapper>();
        services.AddSingleton<IExcelReader, ClosedXmlExcelReader>();
        services.AddSingleton<IHl7MessageFactory, NhapiHl7MessageFactory>();
        services.AddSingleton<IMessageWriter, FileMessageWriter>();
        services.AddHostedService<Worker>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(options =>
        {
            options.IncludeScopes = false;
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
        });
    });

var host = builder.Build();

var bootstrapper = host.Services.GetRequiredService<IFileBootstrapper>();
await bootstrapper.PrepareAsync(CancellationToken.None);

await host.RunAsync();

