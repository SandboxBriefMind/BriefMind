namespace BriefMind.Server.Api.DTOs
{
    public class ProcessAuditRequest
    {
        public string ModelName { get; set; } = "phi3:mini";
        public List<string>? TargetClauseIds { get; set; }
    }
}
