using BriefMind.Server.Domain;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BriefMind.Server.Services
{
    public interface IClauseSegmenter
    {
        SegmentedDocument SegmentAndHash(ExtractedDocument extractedDocument);
    }

    public class ClauseSegmenter : IClauseSegmenter
    {
                // Header = clause number, preceded by start-of-text or sentence-ending punctuation,
        // followed by a capitalised word. Case-sensitive, so "Section 7.1 does not..." never matches.
        private static readonly Regex ClauseHeaderRegex = new Regex(
            @"(?<=(?:^|[.!?;:”""])\s*)(?=(?:\d{1,2}\.\d{1,2}\.?|\d{1,2}\.)\s+[A-Z“""]|(?:SECTION|ARTICLE)\s+(?:\d+|[IVXLC]+)\b)",
            RegexOptions.Compiled);

        private static readonly Regex SectionNameRegex = new Regex(
            @"^(?:(?:SECTION|ARTICLE)\s+(?:\d+|[IVXLC]+)|\d{1,2}\.\d{1,2}|\d{1,2})",
            RegexOptions.Compiled);

        public SegmentedDocument SegmentAndHash(ExtractedDocument doc)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));

            var result = new SegmentedDocument { Metadata = doc.Metadata , FullText = doc.FullText };
            string text = doc.FullText;
            if (string.IsNullOrWhiteSpace(text)) return result;

            var starts = ClauseHeaderRegex.Matches(text).Select(m => m.Index).Distinct().OrderBy(i => i).ToList();
            int sequence = 1;

            if (starts.Count == 0)
            {
                Add("General", 0, text.Length);
                return result;
            }

            if (starts[0] > 0) Add("Preamble", 0, starts[0]);

            for (int i = 0; i < starts.Count; i++)
            {
                int end = i < starts.Count - 1 ? starts[i + 1] : text.Length;
                string raw = text.Substring(starts[i], end - starts[i]).Trim();
                string id = SectionNameRegex.Match(raw) is { Success: true } m ? m.Value : $"Clause {sequence}";

                // Skip heading-only blocks like "1. Services" when the next header is 1.x
                if (i < starts.Count - 1 && raw.Length < 80 && !id.Contains('.'))
                {
                    string nextRaw = text.Substring(starts[i + 1], Math.Min(12, text.Length - starts[i + 1]));
                    if (nextRaw.StartsWith(id + ".")) continue;
                }

                Add(id, starts[i], end);
            }
            return result;

            void Add(string id, int start, int end)
            {
                string raw = text.Substring(start, end - start);
                string trimmed = raw.Trim();
                if (trimmed.Length == 0) return;
                int s = start + (raw.Length - raw.TrimStart().Length);
                result.Clauses.Add(CreateClause(doc.Metadata.DocumentId, sequence++, id, trimmed, s, s + trimmed.Length, doc.Spans));
            }
        }

        private static ContractClause CreateClause(
            string documentId,
            int sequenceIndex,
            string sectionIdentifier,
            string rawText,
            int startOffset,
            int endOffset,
            List<TextSpan> spans)
        {
            // 1. Calculate immutable SHA-256 fingerprint of the clause string
            string hash = ComputeSha256Hash(rawText);

            // 2. Resolve page spans based on offsets
            int startPage = 1;
            int endPage = 1;

            if (spans.Count > 0)
            {
                var startSpan = spans.Find(s => s.StartOffset >= startOffset) ?? spans[0];
                var endSpan = spans.FindLast(s => s.EndOffset <= endOffset) ?? spans[^1];

                startPage = startSpan.PageNumber;
                endPage = endSpan.PageNumber;
            }

            return new ContractClause
            {
                DocumentId = documentId,
                SequenceIndex = sequenceIndex,
                SectionIdentifier = sectionIdentifier,
                RawText = rawText,
                ClauseHashSha256 = hash,
                StartOffset = startOffset,
                EndOffset = endOffset,
                StartPage = startPage,
                EndPage = endPage
            };
        }

        private static string ComputeSha256Hash(string rawInput)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawInput));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}