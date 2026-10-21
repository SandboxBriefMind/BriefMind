namespace BriefMind.Server.Api.DTOs
{
    public class ContractClauseDto
    {
        public string ClauseId { get; set; } = string.Empty;
        public int SequenceIndex { get; set; }
        public string SectionIdentifier { get; set; } = string.Empty;
        public string RawText { get; set; } = string.Empty;
        public string ClauseHashSha256 { get; set; } = string.Empty;
        public int StartOffset { get; set; }
        public int EndOffset { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
    }
}
