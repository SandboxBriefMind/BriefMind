using BriefMind.Server.Domain;
using System.Security.Cryptography;
using System.Text;

namespace BriefMind.Server.Services
{
    public interface IProvenanceService
    {
        ProvenanceRecordUnit CreateRecord(
            ContractClause clause,
            ExtractedDocument document,
            ClauseDebateRecord debateRecord,
            string modelName);

        AuditVerificationBundle CreateBundle(
            ExtractedDocument document,
            List<ProvenanceRecordUnit> records);
    }

    public class ProvenanceService : IProvenanceService
    {
        public ProvenanceRecordUnit CreateRecord(
            ContractClause clause,
            ExtractedDocument document,
            ClauseDebateRecord debateRecord,
            string modelName)
        {
            var record = new ProvenanceRecordUnit
            {
                DocumentId = document.Metadata.DocumentId,
                FileHashSha256 = document.Metadata.FileHashSha256,
                ClauseId = clause.ClauseId,
                SectionIdentifier = clause.SectionIdentifier,
                RawClauseText = clause.RawText,
                ClauseHashSha256 = clause.ClauseHashSha256,
                StartOffset = clause.StartOffset,
                EndOffset = clause.EndOffset,
                StartPage = clause.StartPage,
                EndPage = clause.EndPage,
                ProcessingModel = new ModelDescriptor { ModelName = modelName },
                DebateRecord = debateRecord,
                GeneratedAtUtc = DateTime.UtcNow
            };

            // Compute unique record fingerprint binding document hash, clause hash, and verdict score
            record.RecordFingerprintSha256 = ComputeRecordHash(record);

            return record;
        }

        public AuditVerificationBundle CreateBundle(
            ExtractedDocument document,
            List<ProvenanceRecordUnit> records)
        {
            var bundle = new AuditVerificationBundle
            {
                DocumentMetadata = document.Metadata,
                Records = records,
                ExportedAtUtc = DateTime.UtcNow
            };

            // Compute root signature for the full bundle
            bundle.BundleSignatureSha256 = ComputeBundleHash(bundle);

            return bundle;
        }

        private static string ComputeRecordHash(ProvenanceRecordUnit record)
        {
            // The payload ensures the AI output is explicitly tied to the exact source bytes
            string payload = $"{record.FileHashSha256}:{record.ClauseHashSha256}:{record.StartOffset}:{record.EndOffset}:{record.DebateRecord.Verdict.RiskScore}:{record.DebateRecord.Verdict.IsContested}";
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static string ComputeBundleHash(AuditVerificationBundle bundle)
        {
            var sb = new StringBuilder();
            sb.Append(bundle.DocumentMetadata.FileHashSha256);
            foreach (var record in bundle.Records)
            {
                sb.Append(record.RecordFingerprintSha256);
            }

            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
