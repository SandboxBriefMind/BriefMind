using System.Diagnostics;
using System.Text.Json;
using BriefMind.Server.Domain;

namespace BriefMind.Server.Infrastructure;

public interface IDebateEngine
{
    Task<ClauseDebateRecord> ConductDebateAsync(
        ContractClause clause,
        string defenderModel = "phi3:mini",
        string auditorModel = "qwen2.5:1.5b",
        string judgeModel = "phi3:mini");
}

public class DebateEngine : IDebateEngine
{
    private readonly IOllamaClient _ollamaClient;

    public DebateEngine(IOllamaClient ollamaClient)
    {
        _ollamaClient = ollamaClient;
    }

    public async Task<ClauseDebateRecord> ConductDebateAsync(
        ContractClause clause,
        string defenderModel = "phi3:mini",
        string auditorModel = "qwen2.5:1.5b",
        string judgeModel = "phi3:mini")
    {
        var record = new ClauseDebateRecord
        {
            ClauseId = clause.ClauseId,
            ClauseHashSha256 = clause.ClauseHashSha256
        };

        var stopwatch = new Stopwatch();

        stopwatch.Start();
        var defenderPrompt = $"""
You are legal counsel defending a software/services provider.
Analyze the following contract clause and present a concise argument explaining why this term is standard, commercially reasonable, safe, or legally justified.

SECTION: {clause.SectionIdentifier}
CLAUSE TEXT:
{clause.RawText}

Provide a direct, concise legal defense:
""";

        var defenderResponse = await _ollamaClient.GenerateAsync(defenderModel, defenderPrompt);
        stopwatch.Stop();

        record.DefenderArgument = new DebateArgument
        {
            ModelName = defenderModel,
            Role = DebateRole.Defender,
            ArgumentText = defenderResponse,
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds
        };

        stopwatch.Reset();
        stopwatch.Start();
        var auditorPrompt = $"""
You are an aggressive adversarial legal auditor reviewing a contract on behalf of a customer.
Analyze the following clause and present a concise argument highlighting severe risks, hidden liabilities, short notification windows, or operational constraints for the customer.

SECTION: {clause.SectionIdentifier}
CLAUSE TEXT:
{clause.RawText}

Provide a direct, concise legal risk critique:
""";

        var auditorResponse = await _ollamaClient.GenerateAsync(auditorModel, auditorPrompt);
        stopwatch.Stop();

        record.AuditorArgument = new DebateArgument
        {
            ModelName = auditorModel,
            Role = DebateRole.Auditor,
            ArgumentText = auditorResponse,
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds
        };

        stopwatch.Reset();
        stopwatch.Start();
        var judgePrompt = $$$"""
You are a judicial referee evaluating legal arguments. Output JSON ONLY.

CLAUSE TEXT:
{{{clause.RawText}}}

DEFENDER ARGUMENT:
{{{defenderResponse}}}

AUDITOR ARGUMENT:
{{{auditorResponse}}}

Output a single valid JSON object strictly adhering to this structure:
{{
  "riskScore": <integer 0-100>,
  "isContested": <true if Defender and Auditor disagree on material legal risks, otherwise false>,
  "contestReason": "<short string explaining the core point of disagreement>",
  "verdictSummary": "<concise judicial synthesis>",
  "redlineRecommendation": "<suggested revision text if riskScore > 50, otherwise empty string>"
 }}
""";

        var judgeJsonResponse = await _ollamaClient.GenerateAsync(judgeModel, judgePrompt, jsonMode: true);
        stopwatch.Stop();
        var judgeExecutionTimeMs = stopwatch.ElapsedMilliseconds;

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var parsedVerdict = JsonSerializer.Deserialize<JudgeVerdictDto>(judgeJsonResponse, options);

            record.Verdict = new JudgeVerdict
            {
                ModelName = judgeModel,
                RiskScore = parsedVerdict?.RiskScore ?? 50,
                IsContested = parsedVerdict?.IsContested ?? false,
                ContestReason = parsedVerdict?.ContestReason ?? string.Empty,
                VerdictSummary = parsedVerdict?.VerdictSummary ?? string.Empty,
                RedlineRecommendation = parsedVerdict?.RedlineRecommendation ?? string.Empty,
                ExecutionTimeMs = judgeExecutionTimeMs
            };
        }
        catch (JsonException)
        {
            record.Verdict = new JudgeVerdict
            {
                ModelName = judgeModel,
                RiskScore = 50,
                IsContested = true,
                ContestReason = "JSON parsing fallback triggered.",
                VerdictSummary = judgeJsonResponse,
                ExecutionTimeMs = judgeExecutionTimeMs
            };
        }

        return record;
    }

    private sealed class JudgeVerdictDto
    {
        public int RiskScore { get; set; }
        public bool IsContested { get; set; }
        public string ContestReason { get; set; } = string.Empty;
        public string VerdictSummary { get; set; } = string.Empty;
        public string RedlineRecommendation { get; set; } = string.Empty;
    }
}
