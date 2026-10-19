namespace BriefMind.Server.Domain
{
    public class ClauseDebateRecord
    {
        public string ClauseId { get; set; } = string.Empty;
        public string ClauseHashSha256 { get; set; } = string.Empty;
        public DebateArgument DefenderArgument { get; set; } = new();
        public DebateArgument AuditorArgument { get; set; } = new();
        public JudgeVerdict Verdict { get; set; } = new();
    }
}
