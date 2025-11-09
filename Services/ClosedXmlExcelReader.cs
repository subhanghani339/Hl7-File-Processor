using System.Globalization;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;

namespace Hl7FileProcessor.Services;

public sealed class ClosedXmlExcelReader : IExcelReader
{
    private readonly ILogger<ClosedXmlExcelReader> _logger;

    public ClosedXmlExcelReader(ILogger<ClosedXmlExcelReader> logger)
    {
        _logger = logger;
    }

    public IReadOnlyCollection<PatientRow> ReadRows(Stream excelStream)
    {
        using var workbook = new XLWorkbook(excelStream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet is null)
        {
            throw new InvalidOperationException("The Excel file does not contain any worksheet.");
        }

        var rows = new List<PatientRow>();
        var rowNumber = 2;
        while (!worksheet.Row(rowNumber).IsEmpty())
        {
            try
            {
                var patientRow = ParseRow(worksheet, rowNumber);
                rows.Add(patientRow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse row {RowNumber}.", rowNumber);
                throw;
            }

            rowNumber++;
        }

        return rows;
    }

    private static PatientRow ParseRow(IXLWorksheet worksheet, int rowNumber)
    {
        var patientId = worksheet.Cell(rowNumber, 1).GetString().Trim();
        var firstName = worksheet.Cell(rowNumber, 2).GetString().Trim();
        var lastName = worksheet.Cell(rowNumber, 3).GetString().Trim();
        var dobCell = worksheet.Cell(rowNumber, 4);
        var gender = worksheet.Cell(rowNumber, 5).GetString().Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(patientId))
        {
            throw new InvalidOperationException($"Row {rowNumber} is missing a PatientId value.");
        }

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            throw new InvalidOperationException($"Row {rowNumber} is missing the patient's name.");
        }

        var dateOfBirth = ConvertToDateOnly(dobCell, rowNumber);

        if (gender is not ("M" or "F" or "O"))
        {
            throw new InvalidOperationException($"Row {rowNumber} has an invalid gender value '{gender}'.");
        }

        return new PatientRow(patientId, firstName, lastName, dateOfBirth, gender);
    }

    private static DateOnly ConvertToDateOnly(IXLCell cell, int rowNumber)
    {
        if (cell.DataType == XLDataType.DateTime && cell.GetDateTime() is { } dateTimeValue)
        {
            return DateOnly.FromDateTime(dateTimeValue);
        }

        var rawValue = cell.GetString().Trim();
        if (DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedDateTime))
        {
            return DateOnly.FromDateTime(parsedDateTime);
        }

        throw new InvalidOperationException($"Row {rowNumber} has an invalid DateOfBirth value '{rawValue}'.");
    }
}

