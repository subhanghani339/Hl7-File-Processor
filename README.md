# HL7 File Processor Windows Service

A Windows Service that monitors an input folder for Excel files, processes patient data, and generates HL7 ADT^A01 messages.

## Features

- **Automated File Processing**: Monitors input folder every 5 minutes (configurable)
- **Excel File Support**: Reads patient data from Excel (.xlsx) files
- **HL7 Message Generation**: Creates HL7 ADT^A01 admission messages using NHapi
- **File Archiving**: Moves processed files to a processed subfolder
- **Windows Service**: Runs as a Windows Service for continuous operation

## Requirements

- .NET 8.0 SDK
- Windows OS (for Windows Service support)
- Administrator privileges (for service installation)

## Configuration

Edit `appsettings.json` to configure:

```json
{
  "Processing": {
    "InputFolder": "C:\\HL7Input",
    "OutputFolder": "C:\\HL7Output",
    "PollingIntervalMinutes": 5
  }
}
```

### Configuration Options

- **InputFolder**: Path to folder containing Excel files to process
- **OutputFolder**: Path where HL7 message files will be written
- **PollingIntervalMinutes**: How often to check for new files (in minutes)

### Environment Variables

You can override configuration using environment variables with the `HL7_` prefix:

- `HL7_Processing__InputFolder`
- `HL7_Processing__OutputFolder`
- `HL7_Processing__PollingIntervalMinutes`

## Excel File Format

The service expects Excel files with the following columns (first row is header):

| Column | Description | Example |
|--------|-------------|---------|
| PatientId | Unique patient identifier | P001 |
| FirstName | Patient's first name | Alice |
| LastName | Patient's last name | Smith |
| DateOfBirth | Date of birth | 1985-01-15 |
| Gender | Gender code (M/F/O) | F |

## Building

```bash
dotnet restore
dotnet build
```

## Running as Console Application (Development)

```bash
dotnet run
```

## Installing as Windows Service

1. Build the project:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. Install the service using `sc` command (run as Administrator):
   ```bash
   sc create "HL7 File Processor" binPath="C:\path\to\publish\Hl7FileProcessor.exe"
   sc start "HL7 File Processor"
   ```

3. Or use PowerShell (run as Administrator):
   ```powershell
   New-Service -Name "HL7 File Processor" -BinaryPathName "C:\path\to\publish\Hl7FileProcessor.exe" -StartupType Automatic
   Start-Service "HL7 File Processor"
   ```

## Service Management

- **Start**: `sc start "HL7 File Processor"` or `Start-Service "HL7 File Processor"`
- **Stop**: `sc stop "HL7 File Processor"` or `Stop-Service "HL7 File Processor"`
- **Uninstall**: `sc delete "HL7 File Processor"` or `Remove-Service "HL7 File Processor"`

## How It Works

1. Service starts and creates input/output folders if they don't exist
2. Creates a dummy Excel file (`patients.xlsx`) in the input folder if none exists
3. Every 5 minutes (or configured interval), checks for `.xlsx` files in the input folder
4. For each Excel file:
   - Reads all patient rows
   - Generates an HL7 ADT^A01 message for each patient
   - Writes HL7 messages to the output folder
   - Moves the processed Excel file to `input_folder/processed/` with timestamp prefix

## Output Files

HL7 messages are saved as individual files in the output folder with the naming format:
```
{PatientId}_{Timestamp}.hl7
```

Example: `P001_20250109120000000.hl7`

## Logging

Logs are written to the console. When running as a Windows Service, logs can be viewed using:
- Event Viewer (Windows Logs > Application)
- Or configure file logging in `Program.cs`

## Dependencies

- **ClosedXML**: Excel file reading
- **NHapi**: HL7 message generation
- **Microsoft.Extensions.Hosting.WindowsServices**: Windows Service support

