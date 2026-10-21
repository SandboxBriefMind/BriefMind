namespace BriefMind.Server.Api.DTOs
{
    public class DocumentUploadResponse
    {
        public string DocumentId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileHashSha256 { get; set; } = string.Empty;
        public int TotalPages { get; set; }
        public int TotalClauses { get; set; }
        public string FullText { get; set; } = string.Empty;
        public List<ContractClauseDto> Clauses { get; set; } = new();
    }
}
