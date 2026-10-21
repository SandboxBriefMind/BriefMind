namespace BriefMind.Server.Domain
{
    public class SegmentedDocument
    {
        public DocumentMetadata Metadata { get; set; } = new();
        public string FullText { get; set; } = string.Empty;
        public List<ContractClause> Clauses { get; set; } = new();
    }
}
