namespace BriefMind.Server.Domain
{
    public class DocumentMetadata
    {
        public string DocumentId { get; set; } = Guid.NewGuid().ToString("N");
        public string FileName { get; set; } = string.Empty;
        public string FileHashSha256 { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public int TotalPages { get; set; }
        public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
