using System.Globalization;
using ClosedXML.Excel;
using Hl7FileProcessor.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hl7FileProcessor.Services;

public sealed class FileBootstrapper : IFileBootstrapper
{
    private readonly ILogger<FileBootstrapper> _logger;
    private readonly ProcessingOptions _options;

    public FileBootstrapper(IOptions<ProcessingOptions> options, ILogger<FileBootstrapper> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task PrepareAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_options.InputFolder);
        Directory.CreateDirectory(_options.OutputFolder);

        var dummyFilePath = Path.Combine(_options.InputFolder, "patients.xlsx");
        if (!File.Exists(dummyFilePath))
        {
            CreateDummyWorkbook(dummyFilePath);
            _logger.LogInformation("Created dummy Excel file at {DummyFilePath}.", dummyFilePath);
        }

        return Task.CompletedTask;
    }

    private static void CreateDummyWorkbook(string filePath)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Patients");
        worksheet.Cell(1, 1).Value = "PatientId";
        worksheet.Cell(1, 2).Value = "FirstName";
        worksheet.Cell(1, 3).Value = "LastName";
        worksheet.Cell(1, 4).Value = "DateOfBirth";
        worksheet.Cell(1, 5).Value = "Gender";

        worksheet.Cell(2, 1).Value = "P001";
        worksheet.Cell(2, 2).Value = "Alice";
        worksheet.Cell(2, 3).Value = "Smith";
        worksheet.Cell(2, 4).Value = DateTime.Parse("1985-01-15", CultureInfo.InvariantCulture);
        worksheet.Cell(2, 5).Value = "F";

        worksheet.Cell(3, 1).Value = "P002";
        worksheet.Cell(3, 2).Value = "Bob";
        worksheet.Cell(3, 3).Value = "Johnson";
        worksheet.Cell(3, 4).Value = DateTime.Parse("1979-08-02", CultureInfo.InvariantCulture);
        worksheet.Cell(3, 5).Value = "M";

        worksheet.Cell(4, 1).Value = "P003";
        worksheet.Cell(4, 2).Value = "Carol";
        worksheet.Cell(4, 3).Value = "Diaz";
        worksheet.Cell(4, 4).Value = DateTime.Parse("1993-12-21", CultureInfo.InvariantCulture);
        worksheet.Cell(4, 5).Value = "F";

        worksheet.Columns().AdjustToContents();
        workbook.SaveAs(filePath);
    }
}

