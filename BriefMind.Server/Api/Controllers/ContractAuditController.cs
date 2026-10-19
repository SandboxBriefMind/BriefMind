using BriefMind.Server.Api.DTOs;
using BriefMind.Server.Domain;
using BriefMind.Server.Infrastructure;
using BriefMind.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BriefMind.Server.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractAuditController : Controller
    {
        private readonly IPdfTextExtractor _pdfTextExtractor;
        private readonly IClauseSegmenter _clauseSegmenter;
        private readonly IDebateEngine _debateEngine;
        private readonly IProvenanceService _provenanceService;

        // In-memory caching for demo pipeline state
        private static readonly Dictionary<string, (ExtractedDocument Doc, SegmentedDocument Segmented)> DocumentCache = new();
        private static readonly Dictionary<string, AuditVerificationBundle> BundleCache = new();

        public ContractAuditController(
            IPdfTextExtractor pdfTextExtractor,
            IClauseSegmenter clauseSegmenter,
            IDebateEngine debateEngine,
            IProvenanceService provenanceService)
        {
            _pdfTextExtractor = pdfTextExtractor;
            _clauseSegmenter = clauseSegmenter;
            _debateEngine = debateEngine;
            _provenanceService = provenanceService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public IActionResult UploadDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("A valid PDF file must be uploaded.");

            using var stream = file.OpenReadStream();
            var extractedDoc = _pdfTextExtractor.ExtractTextWithOffsets(stream, file.FileName);
            var segmentedDoc = _clauseSegmenter.SegmentAndHash(extractedDoc);

            DocumentCache[extractedDoc.Metadata.DocumentId] = (extractedDoc, segmentedDoc);

            var response = new DocumentUploadResponse
            {
                DocumentId = extractedDoc.Metadata.DocumentId,
                FileName = extractedDoc.Metadata.FileName,
                FileHashSha256 = extractedDoc.Metadata.FileHashSha256,
                TotalPages = extractedDoc.Metadata.TotalPages,
                TotalClauses = segmentedDoc.Clauses.Count,
                FullText = extractedDoc.FullText,
                Clauses = segmentedDoc.Clauses.Select(c => new ContractClauseDto
                {
                    ClauseId = c.ClauseId,
                    SequenceIndex = c.SequenceIndex,
                    SectionIdentifier = c.SectionIdentifier,
                    RawText = c.RawText,
                    ClauseHashSha256 = c.ClauseHashSha256,
                    StartOffset = c.StartOffset,
                    EndOffset = c.EndOffset,
                    StartPage = c.StartPage,
                    EndPage = c.EndPage
                }).ToList()
            };

            return Ok(response);
        }

        [HttpPost("documents/{documentId}/audit")]
        public async Task<IActionResult> ExecuteAudit(string documentId, [FromBody] ProcessAuditRequest request)
        {
            if (!DocumentCache.TryGetValue(documentId, out var cachedData))
                return NotFound($"Document ID {documentId} not found.");

            var (extractedDoc, segmentedDoc) = cachedData;
            var targetClauses = segmentedDoc.Clauses;

            if (request.TargetClauseIds != null && request.TargetClauseIds.Count > 0)
            {
                targetClauses = targetClauses
                    .Where(c => request.TargetClauseIds.Contains(c.ClauseId))
                    .ToList();
            }

            var recordUnits = new List<ProvenanceRecordUnit>();

            // Process sequentially to keep memory stable on 4GB VRAM
            foreach (var clause in targetClauses)
            {
                var debateRecord = await _debateEngine.ConductDebateAsync(clause, request.ModelName);

                var recordUnit = _provenanceService.CreateRecord(
                    clause,
                    extractedDoc,
                    debateRecord,
                    request.ModelName
                );

                recordUnits.Add(recordUnit);
            }

            var bundle = _provenanceService.CreateBundle(extractedDoc, recordUnits);
            BundleCache[bundle.BundleId] = bundle;

            return Ok(bundle);
        }

        [HttpGet("bundles/{bundleId}/export")]
        public IActionResult ExportBundle(string bundleId)
        {
            if (!BundleCache.TryGetValue(bundleId, out var bundle))
                return NotFound($"Bundle ID {bundleId} not found.");

            string jsonBytes = JsonSerializer.Serialize(bundle, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(jsonBytes);
            string downloadFileName = $"AuditBundle_{bundle.DocumentMetadata.FileName}_{bundle.BundleId[..8]}.json";

            return File(fileBytes, "application/json", downloadFileName);
        }
    }
}
