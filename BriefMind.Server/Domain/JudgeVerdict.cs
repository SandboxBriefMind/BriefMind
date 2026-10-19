namespace BriefMind.Server.Domain
{
    public class JudgeVerdict
    {
        public string ModelName { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public bool IsContested { get; set; }
        public string ContestReason { get; set; } = string.Empty;
        public string VerdictSummary { get; set; } = string.Empty;
        public string RedlineRecommendation { get; set; } = string.Empty;
        public long ExecutionTimeMs { get; set; }
    }
}
