namespace BriefMind.Server.Api.DTOs
{
    public class ProcessAuditRequest
    {
        public string ModelName { get; set; } = "phi3:mini";
        public string? DefenderModel { get; set; }
        public string? AuditorModel { get; set; }
        public string? JudgeModel { get; set; }
        public List<string>? TargetClauseIds { get; set; }
    }
}
