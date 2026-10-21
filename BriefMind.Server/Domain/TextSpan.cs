using UglyToad.PdfPig.Content;

namespace BriefMind.Server.Domain
{
    public class TextSpan
    {
        public int PageNumber { get; set; }
        public int StartOffset { get; set; }
        public int EndOffset { get; set; }
        public string Text { get; set; } = string.Empty;
        public BoundingBox? BoundingBox { get; set; }
    }
}
