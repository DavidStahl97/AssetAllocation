using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iText.Kernel.Geom;
using System.Reflection;
using CommunityToolkit.Diagnostics;

namespace AssetAllocation.PdfExtraction
{
    public record BoundingBoxInPercentage(float X, float Y, float Width, float Height);

    public static class ReaderExtensions
    {
        private static readonly FieldInfo _locationalResultField = typeof(LocationTextExtractionStrategy).GetField("locationalResult", BindingFlags.NonPublic | BindingFlags.Instance)!;

        public static string ExtractText(this PdfPage page, BoundingBoxInPercentage boundingBox)
        {
            var textEventListener = new LocationTextExtractionStrategy();
            PdfTextExtractor.GetTextFromPage(page, textEventListener);

            var rectangle = CalculateRectangle(page, boundingBox);

            var extractedText = textEventListener.GetResultantText(rectangle);
            return extractedText;
        }

        private static Rectangle CalculateRectangle(PdfPage page, BoundingBoxInPercentage boundingBox) 
        {
            var pageRectangle = page.GetPageSize();
            var pageWidth = pageRectangle.GetWidth();
            var pageHeight = pageRectangle.GetHeight();

            Guard.IsNotEqualTo(pageWidth, 0);
            Guard.IsNotEqualTo(pageHeight, 0);

            var x = boundingBox.X * pageWidth / 100.0f;
            var y = boundingBox.Y * pageHeight / 100.0f;
            var width = boundingBox.Width * pageWidth / 100.0f;
            var height = boundingBox.Height * pageHeight / 100.0f;

            var inverseY = pageHeight - y - height;

            return new Rectangle(x, inverseY, width, height);
        }

        private static string GetResultantText(this LocationTextExtractionStrategy strategy, Rectangle rect)
        {
            var locationalResult = (IList<TextChunk>)_locationalResultField.GetValue(strategy)!;
            var nonMatching = new List<TextChunk>();
            foreach (TextChunk chunk in locationalResult)
            {
                ITextChunkLocation location = chunk.GetLocation();
                Vector start = location.GetStartLocation();
                Vector end = location.GetEndLocation();
                if (!rect.IntersectsLine(start.Get(Vector.I1), start.Get(Vector.I2), end.Get(Vector.I1), end.Get(Vector.I2)))
                {
                    nonMatching.Add(chunk);
                }
            }
            nonMatching.ForEach(c => locationalResult.Remove(c));
            try
            {
                return strategy.GetResultantText();
            }
            finally
            {
                nonMatching.ForEach(c => locationalResult.Add(c));
            }
        }
    }
}
