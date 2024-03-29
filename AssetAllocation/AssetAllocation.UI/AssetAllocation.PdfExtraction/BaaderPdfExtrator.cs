﻿using AssetAllocation.Domain.Models;
using iText.Kernel.Pdf;
using System.Collections.Immutable;

namespace AssetAllocation.PdfExtraction
{
    public class BaaderPdfExtractor(PdfExtractionSettings settings) : IPdfExtractor
    {
        public async Task<PdfExtractionResult> ReadPdfFileAsync(string filePath)
            => await Task.Run(() =>
            {
                try
                {
                    return ReadPdfFile(filePath);
                }
                catch
                {
                    return new ExtractionError(UnexpectedError.Instance);
                }
            });

        private PdfExtractionResult ReadPdfFile(string filePath)
        {
            using var reader = new PdfReader(filePath);
            using var pdfDoc = new PdfDocument(reader);

            var numberOfPages = pdfDoc.GetNumberOfPages();

            if (numberOfPages - settings.NumberOfPageSkips <= 0)
            {
                return new ExtractionError(CannotFindAnySecurities.Instance);
            }
            
            foreach (var pageIndex in Enumerable.Range(1, numberOfPages).Skip(settings.NumberOfPageSkips))
            {
                var page = pdfDoc.GetPage(pageIndex);

                var assets = ExtractAssets(page);
            }

            return null;
        }        

        private List<Asset> ExtractAssets(PdfPage page)
        {
            var assetColumText = page.ExtractText(new BoundingBoxInPercentage(
                X: settings.MarginLeft,
                Y: settings.MarginTop,
                Width: settings.AssetColumnLength,
                Height: 100 - settings.MarginTop));

            var lines = assetColumText.Split('\n');

            var assets = new List<Asset>();

            string assetDescription = string.Empty;

            foreach (var line in lines)
            {
                var isISIN = line.Length >= 12 && 
                    LuhnAlgorithm.IsValidISIN(line[..12]);

                if (!isISIN)
                {
                    assetDescription += string.IsNullOrEmpty(assetDescription) ?
                        line : $" {line}";

                    continue;
                }

                var asset = new Asset(
                    ISIN: line[..12],
                    Description: assetDescription);

                assets.Add(asset);
                assetDescription = string.Empty;
            }

            return assets;
        }
    }
}
