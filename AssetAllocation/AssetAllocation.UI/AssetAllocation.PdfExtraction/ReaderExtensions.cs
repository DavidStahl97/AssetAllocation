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
using System.Security.Cryptography.X509Certificates;

namespace AssetAllocation.PdfExtraction
{
    public record TextLine(string Text, 
        System.Numerics.Vector2 StartPoint,
        System.Numerics.Vector2 EndPoint);

    public static class ReaderExtensions
    {
        private static readonly FieldInfo _locationalResultField = typeof(LocationTextExtractionStrategy).GetField("locationalResult", BindingFlags.NonPublic | BindingFlags.Instance)!;

        public static List<TextLine> ExtractText(this PdfPage page, BoundingBoxInPercentage boundingBox)
        {
            var rectangle = CalculateRectangle(page, boundingBox);
            var extractedText = GetResultantText(page, rectangle);
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

        private static List<TextLine> GetResultantText(PdfPage page, Rectangle rect)
        {
            var strategy = new LocationTextExtractionStrategy();
            PdfTextExtractor.GetTextFromPage(page, strategy);

            var locationalResult = (IList<TextChunk>)_locationalResultField.GetValue(strategy)!;
            var lines = new List<List<TextChunk>>();
            foreach (TextChunk chunk in locationalResult)
            {
                ITextChunkLocation location = chunk.GetLocation();
                Vector start = location.GetStartLocation();
                Vector end = location.GetEndLocation();
                if (rect.IntersectsLine(start.Get(Vector.I1), start.Get(Vector.I2), end.Get(Vector.I1), end.Get(Vector.I2)))
                {
                    var line = lines.Find(x => x[0].GetLocation().SameLine(chunk.GetLocation()));

                    if (line is null)
                    {
                        lines.Add([chunk]);
                    }
                    else
                    {
                        line.Add(chunk);
                    }
                }
            }

            var textLines = lines.Select(line =>
            {
                var location = line[0].GetLocation();
                var startPoint = location.GetStartLocation().ConvertPointToPercentage(page);
                var endPoint = location.GetEndLocation().ConvertPointToPercentage(page);

                var text = string.Join(' ', line.Select(x => x.GetText()));
                return new TextLine(text, startPoint, endPoint);
            })
                .OrderBy(x => x.StartPoint.Y)
                .ToList();

            return textLines;
        }

        private static System.Numerics.Vector2 ConvertPointToPercentage(this Vector point,
            PdfPage page)
        {
            var pageRectangle = page.GetPageSize();
            var pageWidth = pageRectangle.GetWidth();
            var pageHeight = pageRectangle.GetHeight();

            var x = (point.Get(0) / pageWidth) * 100;
            var y = (point.Get(1) / pageHeight) * 100;
            var invertedY = 100 - y;

            return new System.Numerics.Vector2(x, invertedY);
        }
    }
}
