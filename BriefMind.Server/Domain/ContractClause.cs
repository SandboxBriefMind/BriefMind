namespace BriefMind.Server.Domain
{
   public class ContractClause
    {
        public string ClauseId { get; set; } = Guid.NewGuid().ToString("N");
        public string DocumentId { get; set; } = string.Empty;
        public int SequenceIndex { get; set; }
        
        // Structural Metadata
        public string SectionIdentifier { get; set; } = string.Empty; // e.g., "Section 14.2", "12.1", "Article IV"
        public string Title { get; set; } = string.Empty;
        public string RawText { get; set; } = string.Empty;
        
        // Cryptographic Anchor
        public string ClauseHashSha256 { get; set; } = string.Empty;
        
        // Character Offset Range in Full Document
        public int StartOffset { get; set; }
        public int EndOffset { get; set; }
        
        // Visual Page Mapping
        public int StartPage { get; set; }
        public int EndPage { get; set; }
    }
}
