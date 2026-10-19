namespace BriefMind.Server.Domain;

public class ProvenanceRecordUnit
{
    public string RecordId { get; set; } = Guid.NewGuid().ToString("N");
    public string DocumentId { get; set; } = string.Empty;
    public string FileHashSha256 { get; set; } = string.Empty;

    public string ClauseId { get; set; } = string.Empty;
    public string SectionIdentifier { get; set; } = string.Empty;
    public string RawClauseText { get; set; } = string.Empty;
    public string ClauseHashSha256 { get; set; } = string.Empty;
    public int StartOffset { get; set; }
    public int EndOffset { get; set; }
    public int StartPage { get; set; }
    public int EndPage { get; set; }

    public ModelDescriptor ProcessingModel { get; set; } = new();
    public ClauseDebateRecord DebateRecord { get; set; } = new();

    public string RecordFingerprintSha256 { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}
