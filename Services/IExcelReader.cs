namespace Hl7FileProcessor.Services;

public interface IExcelReader
{
    IReadOnlyCollection<PatientRow> ReadRows(Stream excelStream);
}

