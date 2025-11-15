using NHapi.Base.Parser;
using NHapi.Model.V251.Message;
using NHapi.Model.V251.Segment;

namespace Hl7FileProcessor.Services;

public sealed class NhapiHl7MessageFactory : IHl7MessageFactory
{
    private readonly PipeParser _parser;

    public NhapiHl7MessageFactory()
    {
        _parser = new PipeParser();
    }

    public Hl7Message CreateAdmissionMessage(PatientRow row)
    {
        var message = new ADT_A01();

        PopulateMsh(message.MSH);
        PopulatePid(message.PID, row);
        PopulatePv1(message.PV1);

        var serialized = _parser.Encode(message);
        var fileName = $"{row.PatientId}_{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}.hl7";

        return new Hl7Message(fileName, serialized);
    }

    private static void PopulateMsh(MSH msh)
    {
        msh.FieldSeparator.Value = "|";
        msh.EncodingCharacters.Value = "^~\\&";
        msh.SendingApplication.NamespaceID.Value = "HL7FileProcessor";
        msh.ReceivingApplication.NamespaceID.Value = "HL7Consumer";
        msh.DateTimeOfMessage.Time.Value = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        msh.MessageType.MessageCode.Value = "ADT";
        msh.MessageType.TriggerEvent.Value = "A01";
        msh.MessageType.MessageStructure.Value = "ADT_A01";
        msh.MessageControlID.Value = Guid.NewGuid().ToString();
        msh.ProcessingID.ProcessingID.Value = "P";
        msh.VersionID.VersionID.Value = "2.5.1";
    }

    private static void PopulatePid(PID pid, PatientRow row)
    {
        var patientIdentifierList = pid.GetPatientIdentifierList(0);
        patientIdentifierList.IDNumber.Value = row.PatientId;
        patientIdentifierList.AssigningAuthority.NamespaceID.Value = "HFP";
        patientIdentifierList.IdentifierTypeCode.Value = "MR";

        var patientName = pid.GetPatientName(0);
        patientName.FamilyName.Surname.Value = row.LastName;
        patientName.GivenName.Value = row.FirstName;

        pid.DateTimeOfBirth.Time.Value = row.DateOfBirth.ToString("yyyyMMdd");
        pid.AdministrativeSex.Value = row.Gender;
    }

    private static void PopulatePv1(PV1 pv1)
    {
        pv1.PatientClass.Value = "O";
        pv1.AssignedPatientLocation.Facility.NamespaceID.Value = "HFP";
        pv1.AdmissionType.Value = "R";
    }
}

