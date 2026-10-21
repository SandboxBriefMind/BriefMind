namespace BriefMind.Server.Domain
{
    public class ExtractedDocument
    {
        public DocumentMetadata Metadata { get; set; } = new();
        public string FullText { get; set; } = string.Empty;
        public List<TextSpan> Spans { get; set; } = new();
    }
}
