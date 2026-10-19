using BriefMind.Server.Domain;
using System.Security.Cryptography;
using System.Text;
using UglyToad.PdfPig;

namespace BriefMind.Server.Services
{
    public interface IPdfTextExtractor
    {
        ExtractedDocument ExtractTextWithOffsets(Stream pdfStream, string fileName);
    }

    public class PdfTextExtractor : IPdfTextExtractor
    {
        public ExtractedDocument ExtractTextWithOffsets(Stream pdfStream, string fileName)
        {
            if (pdfStream == null || pdfStream.Length == 0)
                throw new ArgumentException("Invalid PDF stream provided.", nameof(pdfStream));

            // Compute file SHA-256 hash first for immutable document identification
            pdfStream.Position = 0;
            string fileHash;
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(pdfStream);
                fileHash = Convert.ToHexString(hashBytes).ToLowerInvariant();
            }
            pdfStream.Position = 0;

            var extracted = new ExtractedDocument();
            extracted.Metadata.FileName = fileName;
            extracted.Metadata.FileSizeBytes = pdfStream.Length;
            extracted.Metadata.FileHashSha256 = fileHash;

            var fullTextBuilder = new StringBuilder();
            var spans = new List<TextSpan>();
            int currentGlobalOffset = 0;

            using (var pdfDocument = PdfDocument.Open(pdfStream))
            {
                extracted.Metadata.TotalPages = pdfDocument.NumberOfPages;

                for (int pageNum = 1; pageNum <= pdfDocument.NumberOfPages; pageNum++)
                {
                    var page = pdfDocument.GetPage(pageNum);
                    var words = page.GetWords();

                    foreach (var word in words)
                    {
                        string wordText = word.Text + " ";
                        int start = currentGlobalOffset;
                        int end = start + wordText.Length;

                        fullTextBuilder.Append(wordText);

                        spans.Add(new TextSpan
                        {
                            PageNumber = pageNum,
                            StartOffset = start,
                            EndOffset = end,
                            Text = wordText.Trim(),
                            BoundingBox = new BoundingBox
                            {
                                Left = word.BoundingBox.Left,
                                Bottom = word.BoundingBox.Bottom,
                                Right = word.BoundingBox.Right,
                                Top = word.BoundingBox.Top
                            }
                        });

                        currentGlobalOffset = end;
                    }

                    // Add page break separator to ensure offset isolation across pages
                    fullTextBuilder.AppendLine();
                    currentGlobalOffset += Environment.NewLine.Length;
                }
            }

            extracted.FullText = fullTextBuilder.ToString();
            extracted.Spans = spans;

            return extracted;
        }
    }
}
