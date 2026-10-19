namespace BriefMind.Server.Domain;

public class AuditVerificationBundle
{
    public string BundleId { get; set; } = Guid.NewGuid().ToString("N");
    public string SchemaVersion { get; set; } = "1.0.0";
    public DocumentMetadata DocumentMetadata { get; set; } = new();
    public List<ProvenanceRecordUnit> Records { get; set; } = new();
    public string BundleSignatureSha256 { get; set; } = string.Empty;
    public DateTime ExportedAtUtc { get; set; } = DateTime.UtcNow;
}
