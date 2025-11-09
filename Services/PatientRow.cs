namespace Hl7FileProcessor.Services;

public sealed record PatientRow(
    string PatientId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender);

