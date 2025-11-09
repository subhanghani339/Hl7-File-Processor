namespace Hl7FileProcessor.Services;

public interface IHl7MessageFactory
{
    Hl7Message CreateAdmissionMessage(PatientRow row);
}

