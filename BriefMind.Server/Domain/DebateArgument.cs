namespace BriefMind.Server.Domain;

public class DebateArgument
{
    public string ModelName { get; set; } = string.Empty;
    public DebateRole Role { get; set; }
    public string ArgumentText { get; set; } = string.Empty;
    public long ExecutionTimeMs { get; set; }
}
